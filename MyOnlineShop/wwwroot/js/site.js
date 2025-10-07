
let selectedDealId = null;

const buttons = document.querySelectorAll('[data-dealid]');
buttons.forEach(btn => {
    btn.addEventListener('click', function () {
        selectedDealId = this.getAttribute('data-dealid');
    });
});

document.getElementById("confirmCancelBtn").addEventListener("click", function () {
    if (selectedDealId) {
        document.getElementById("hiddenDealId").value = selectedDealId;
    }
    document.getElementById("cancelForm").submit();
});


function checkPasswordStrength() {
    const password = document.getElementById("password").value;
    const strengthText = document.getElementById("passwordStrength");
    const strengthHidden = document.getElementById("pw_strength");
    let strength = 0;

    // Prüfkriterien
    if (password.length >= 8) strength++;                     // Länge
    if (/[A-Z]/.test(password)) strength++;                   // Großbuchstaben
    if (/[a-z]/.test(password)) strength++;                   // Kleinbuchstaben
    if (/[0-9]/.test(password)) strength++;                   // Zahl
    if (/[^A-Za-z0-9]/.test(password)) strength++;            // Sonderzeichen

    // Stärke bestimmen
    let message = "";
    let color = "";

    switch (strength) {
        case 0:
        case 1:
            message = "Passwort Sehr schwach";
            color = "red";
            break;
        case 2:
            message = "Passwort Schwach";
            color = "orange";
            break;
        case 3:
            message = "Passwort Mittel";
            color = "gold";
            break;
        case 4:
            message = "Passwort Stark";
            color = "green";
            break;
        case 5:
            message = "Passwort Sehr stark";
            color = "darkgreen";
            break;
    }

    // Anzeige aktualisieren
    strengthText.textContent = message;
    strengthHidden.value = strength; // Verstecktes Feld aktualisieren
    strengthText.style.color = color;
}


//$(document).ready(function () {
//    $("#search_query").autocomplete({
//        minLength: 2,
//        source: function (request, response) {
//            $.ajax({
//                url: `${window.location.origin}/api/SearchSuggestions`,
//                dataType: "json",
//                data: { term: request.term },
//                success: function (data) {
//                    // Wenn du Strings bekommst:
//                    response(data);
//                    // Wenn du Objekte bekommst:
//                    // response(data.map(x => x.name));
//                },
//                error: function () {
//                    response([]);
//                }
//            });
//        }
//    });
//});
