
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