using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Autodesk.Forge.Sample.ExtractCompoundStructureLayers
{
    [Transaction(TransactionMode.Manual)]
    public class Commands : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        private void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            e.Succeeded = true;
            compoundStructureLayeToJson(e.DesignAutomationData);
        }

        private void compoundStructureLayeToJson(DesignAutomationData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Application rvtApp = data.RevitApp;
            if (rvtApp == null)
            {
                throw new InvalidDataException(nameof(rvtApp));
            }

            string modelPath = data.FilePath;
            if (String.IsNullOrWhiteSpace(modelPath))
            {
                throw new InvalidDataException(nameof(modelPath));
            }

            Document doc = data.RevitDoc;
            if (doc == null)
            {
                throw new InvalidOperationException("Could not open document.");
            }

            JArray wallInfoResult = new JArray();

            FilteredElementCollector wallCollector = new FilteredElementCollector(doc).OfClass(typeof(Wall)).WhereElementIsNotElementType();
            foreach (Element e in wallCollector)
            {
                Wall w = e as Wall;
                WallType wtype = w.WallType;

                //CURTAIN WALL doesn't contain compound structure layers
                if (wtype != null && !wtype.Name.Contains("Curtain"))
                {
                    CompoundStructure compoundStruct = wtype.GetCompoundStructure();
                    IList<CompoundStructureLayer> compoundStructLayers = compoundStruct.GetLayers() as IList<CompoundStructureLayer>;
                    if (compoundStructLayers.Count == 0) continue;

                    dynamic wallLayersInfo = new JObject();
                    wallLayersInfo.uniqueId = w.UniqueId;
                    wallLayersInfo.layers = new JArray();

                    foreach (CompoundStructureLayer compoundStructLayer in compoundStructLayers)
                    {
                        dynamic layerInfo = new JObject();
                        layerInfo.function = compoundStructLayer.Function.ToString();
                        layerInfo.material = ((Material)doc.GetElement(compoundStructLayer.MaterialId)).Name;
                        layerInfo.width = compoundStructLayer.Width;

                        wallLayersInfo.layers.Add(layerInfo);
                    }

                    wallInfoResult.Add(wallLayersInfo);
                }
            }

            // save all to a .json file
            using (StreamWriter file = File.CreateText("result.json"))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                wallInfoResult.WriteTo(writer);
            }
        }
    }
}
