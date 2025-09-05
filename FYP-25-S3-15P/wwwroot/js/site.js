// =======================
// SubscriptionPlans dashboard scripts
// =======================

// Re-open modal if server-side validation failed
(function () {
    const reopen = document.body.getAttribute("data-active-modal");
    if (reopen === "add") {
        new bootstrap.Modal(document.getElementById("addPlanModal")).show();
    }
    if (reopen === "edit") {
        new bootstrap.Modal(document.getElementById("viewEditModal")).show();
    }
})();

// View/Edit modal population
const viewEditModal = document.getElementById("viewEditModal");
if (viewEditModal) {
    viewEditModal.addEventListener("show.bs.modal", function (event) {
        const btn = event.relatedTarget;
        document.getElementById("edit_PlanID").value = btn.dataset.id;
        document.getElementById("edit_Name").value = btn.dataset.name ?? "";
        document.getElementById("edit_Price").value = btn.dataset.price ?? "";
        document.getElementById("edit_Description").value = btn.dataset.description ?? "";
        document.getElementById("edit_Features").value = (btn.dataset.features || "").replace(/\\n/g, "\n");

        setEditEnabled(false);
    });
}

// Toggle enable/disable in View/Edit modal
const enableBtn = document.getElementById("enableEditBtn");
if (enableBtn) {
    enableBtn.addEventListener("click", function () {
        const isDisabled = document.getElementById("edit_Name").readOnly;
        setEditEnabled(isDisabled);
    });
}

function setEditEnabled(enabled) {
    ["edit_Name", "edit_Price", "edit_Description", "edit_Features"].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.readOnly = !enabled;
    });
    const saveBtn = document.getElementById("saveChangesBtn");
    if (saveBtn) saveBtn.disabled = !enabled;
}

// Delete modal population
const deleteModal = document.getElementById("deleteModal");
if (deleteModal) {
    deleteModal.addEventListener("show.bs.modal", function (event) {
        const btn = event.relatedTarget;
        document.getElementById("delete_Id").value = btn.dataset.id;
        document.getElementById("delete_Name").textContent = btn.dataset.name ?? "";
    });
}
