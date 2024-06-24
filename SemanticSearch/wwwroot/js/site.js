$(document).ready(function () {
    $("#searchForm").on("submit", function (event) {
        event.preventDefault();
        const data = $("#txtData").val();
        const query = $("#txtQuery").val();

        // Clear the results.
        $("#searchResult").empty();
        $('#similaritiesTable').hide();
        $('#similaritiesTable tbody').empty();

        // Show a progress spinner.
        $("#spinner").show();

        // Call the API.
        $.ajax({
            url: "/Index?handler=Search",
            type: "POST",
            data: { data, query, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() },
            success: function (data) {
                // Show the top matching search result.
                $("#searchResult").html(data.result);

                // Show the list of similarities.
                $.each(data.similarities, function(i, item) {
                    // Set a color intensity for this score.
                    const color = 255 * item.score;

                    // Set the color to red or green according to match.
                    const colorStyle = `rgb(${item.score < 0.5 ? `${color}, 0` : `0, ${color}`}, 0)`;

                    // Insert the row in the
                    var row = '<tr><td>' + item.phrase + `</td><td style='color: ${colorStyle};'>` + item.score + '</td></tr>';
                    $('#similaritiesTable tbody').append(row);
                });

                $('#similaritiesTable').show();
            },
            error: function(jqXHR, textStatus, errorThrown) {
                console.error("An error occurred: ", textStatus, errorThrown);
                alert("An error occurred while processing your request. Please try again.");
            },
            complete: function() {
                $("#spinner").hide();
            }
        });
    });
});