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

// ----- Features tab wiring (Choices.js + edit modal) -------------------------
(function () {
    function initFeaturesTab() {
        // Selects exist only on the PA dashboard "Features" tab
        const addSel  = document.querySelector('#add_PlanIds');
        const editSel = document.querySelector('#f_PlanIds');
        const editModal = document.getElementById('editFeatureModal');

        // Initialize Choices.js if the library is present and not already initialized
        if (window.Choices && addSel && !addSel.dataset.choices) {
            addSel.dataset.choices = '1';
            window._featureAddChoices = new Choices(addSel, {
                removeItemButton: true,
                shouldSort: false,
                searchEnabled: true
            });
        }

        if (window.Choices && editSel && !editSel.dataset.choices) {
            editSel.dataset.choices = '1';
            window._featureEditChoices = new Choices(editSel, {
                removeItemButton: true,
                shouldSort: false,
                searchEnabled: true
            });
        }

        // Wire the edit modal once
        if (editModal && !editModal.dataset.wired) {
            editModal.addEventListener('show.bs.modal', function (ev) {
                const btn = ev.relatedTarget;
                if (!btn) return;

                // Fill basic fields
                editModal.querySelector('#f_Id').value          = btn.dataset.id || '';
                editModal.querySelector('#f_Name').value        = btn.dataset.name || '';
                editModal.querySelector('#f_Description').value = btn.dataset.description || '';

                // Preselect plans
                const ids = (btn.dataset.planids || '')
                    .split(',')
                    .map(s => s.trim())
                    .filter(Boolean);

                if (window._featureEditChoices) {
                    window._featureEditChoices.removeActiveItems();
                    window._featureEditChoices.setChoiceByValue(ids); // accepts array of strings
                } else if (editSel) {
                    // Fallback if Choices failed to load
                    [...editSel.options].forEach(o => o.selected = ids.includes(o.value));
                }
            });

            editModal.dataset.wired = '1';
        }
    }

    // Run on every full page load
    document.addEventListener('DOMContentLoaded', initFeaturesTab);
})();

// =======================
// Applications dashboard (read-only + decide)
// =======================
(function () {
    const appModal = document.getElementById("viewAppModal");
    if (!appModal) return;

    appModal.addEventListener("show.bs.modal", function (ev) {
        const btn = ev.relatedTarget;
        if (!btn) return;

        document.getElementById("app_Name").value    = btn.dataset.name || "";
        document.getElementById("app_Email").value   = btn.dataset.email || "";
        document.getElementById("app_UniName").value = btn.dataset.uniname || "";
        document.getElementById("app_UEN").value     = btn.dataset.uen || "";
        document.getElementById("app_Role").value    = btn.dataset.role || "";
        document.getElementById("app_Plan").value    = btn.dataset.plan || "";
        document.getElementById("app_Status").value  = btn.dataset.status || "";

        const id = btn.dataset.id || "";
        const approve = document.getElementById("approve_Id");
        const reject  = document.getElementById("reject_Id");
        if (approve) approve.value = id;
        if (reject)  reject.value  = id;
    });
})();





