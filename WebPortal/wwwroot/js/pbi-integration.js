window.ShowMyPowerBI = {
    showReport: function (reportContainer, accessToken, embedUrl, embedReportId) {
        // Get models. models contains enums that can be used.
        var models = window['powerbi-client'].models;
        var config = {
            type: 'report',
            tokenType: models.TokenType.Aad,
            accessToken: accessToken,
            embedUrl: embedUrl,
            id: embedReportId,
            permissions: models.Permissions.All,
            settings: {
                filterPaneEnabled: true,
                navContentPaneEnabled: true,
                bars: {
                    actionBar: {
                        visible: true
                    }
                }
            }
        };
        // Embed the report and display it within the div container.
        powerbi.embed(reportContainer, config);
    },
};