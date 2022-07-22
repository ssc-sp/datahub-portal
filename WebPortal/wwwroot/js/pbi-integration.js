window.ShowMyPowerBI = {
    showReport: function (reportContainer, accessToken, embedUrl, embedReportId, settings) {
        // Get models. models contains enums that can be used.
        var models = window['powerbi-client'].models;
        var config = {
            type: 'report',
            tokenType: models.TokenType.Aad,
            accessToken: accessToken,
            embedUrl: embedUrl,
            id: embedReportId,
            permissions: models.Permissions.All,
            settings: settings
        };
        // Embed the report and display it within the div container.
        var report = powerbi.embed(reportContainer, config);

        report.off("loaded");

        // Triggers when a report schema is successfully loaded
        report.on("loaded", function () {
            console.log("Report load successful");
        });

        // Clear any other rendered handler events
        report.off("rendered");

        // Triggers when a report is successfully embedded in UI
        report.on("rendered", function () {
            console.log("Report render successful");
        });

        // Clear any other error handler events
        report.off("error");

        // Handle embed errors
        report.on("error", function (event) {
            var errorMsg = event.detail;

            // Use errorMsg variable to log error in any destination of choice
            console.error(errorMsg);
            return;
        });
    },
    showReportEmbed: function (reportContainer, accessToken, embedUrl, embedReportId, settings) {
        // Get models. models contains enums that can be used.
        var models = window['powerbi-client'].models;
        var config = {
            type: 'report',
            tokenType: models.TokenType.Embed,
            accessToken: accessToken,
            embedUrl: embedUrl,
            id: embedReportId,
            settings: settings
        };
        // Embed the report and display it within the div container.
        var report = powerbi.embed(reportContainer, config);

        report.off("loaded");

        // Triggers when a report schema is successfully loaded
        report.on("loaded", function () {
            console.log("Report load successful");
        });

        // Clear any other rendered handler events
        report.off("rendered");

        // Triggers when a report is successfully embedded in UI
        report.on("rendered", function () {
            console.log("Report render successful");
        });

        // Clear any other error handler events
        report.off("error");

        // Handle embed errors
        report.on("error", function (event) {
            var errorMsg = event.detail;

            // Use errorMsg variable to log error in any destination of choice
            console.error(errorMsg);
            return;
        });

    },
};