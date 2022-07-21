// Migarte to new syntax...

// *******************************************
// Custom Property Panel
// *******************************************
class CustomPropertyPanel extends Autodesk.Viewing.Extensions.ViewerPropertyPanel {
    constructor(viewer, options) {
        super(viewer, options);
        this._data = null;
    }
    setAggregatedProperties(propertySet) {
        Autodesk.Viewing.Extensions.ViewerPropertyPanel.prototype.setAggregatedProperties.call(this, propertySet);

        if (this._data !== null) {
            this.viewer.getProperties(this.propertyNodeId, (props) => {
                const externalId = props.externalId;

                this._data.forEach((d) => {
                    if (d.uniqueId !== externalId) return;

                    d.layers.forEach((s) => {
                        this.addProperty(s.material, s.width, 'Compound Layer');
                    });
                });
            });
        }
    }

    setNodeProperties(nodeId) {
        Autodesk.Viewing.Extensions.ViewerPropertyPanel.prototype.setNodeProperties.call(this, nodeId);
        this.nodeId = nodeId; // store the dbId for later use
    }
}

// *******************************************
// Custom Property Panel Extension
// *******************************************
class CustomPropertyPanelExtension extends Autodesk.Viewing.Extension {
    constructor(viewer, options) {
        super(viewer, options);
        this._connection = null;
        this._connectionId = null;
        this._panel = null;
        this._data = null;
    }

    load() {
        if (this.options === null) return; // not a supported format
        this.startConnection(() => {
            var params = this.options.itemId.split('/');
            this.options.projectId = params[params.length - 3];

            jQuery.post({
                url: '/api/styles',
                contentType: 'application/json',
                data: JSON.stringify({ 'connectionId': this._connectionId, 'itemId': this.options.itemId, 'versionId': this.options.versionId }),
                success: function (res) {
                    $.notify("Requesting compound structure layer information... please wait.", "info");
                },
                error: function (err) {

                }
            });
        });

        return true;
    }

    unload() {
        if (this._panel === null) return;
        var ext = this.viewer.getExtension('Autodesk.PropertiesManager');
        this._panel = null;
        ext.setDefaultPanel();
        return true;
    }

    onToolbarCreated() {
        this._panel = new CustomPropertyPanel(this.viewer, this.options);
        this.viewer.addEventListener(Autodesk.Viewing.EXTENSION_LOADED_EVENT, (e) => {
            if (e.extensionId !== 'Autodesk.PropertiesManager') return;
            var ext = this.viewer.getExtension('Autodesk.PropertiesManager');
            ext.setPanel(this._panel);
        })
    }

    startConnection(onReady) {
        if (this._connection && this._connection.connectionState) { if (onReady) onReady(); return; }
        this._connection = new signalR.HubConnectionBuilder().withUrl("/api/signalr/designautomation").build();
        this._connection.start()
            .then(() => {
                this._connection.invoke('getConnectionId')
                    .then((id) => {
                        this._connectionId = id; // we'll need this...
                        if (onReady) onReady();
                    });
            });

        this._connection.on("propsReady", (url) => {
            jQuery.get({
                url: url,
                success: (res) => {
                    this._panel._data = JSON.parse(res);
                    $.notify("Compound structure layer information ready to use.", "success");
                },
                error: (err) => {

                }
            });
        });

        this._connection.on("onComplete", (reportUrl) => {
            console.log(reportUrl);
        });
    }
}

Autodesk.Viewing.theExtensionManager.registerExtension('Autodesk.Sample.CustomPropertyPanelExtension', CustomPropertyPanelExtension);