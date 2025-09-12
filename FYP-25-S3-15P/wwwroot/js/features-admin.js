// =============== Features Admin (PA Dashboard) ==================
// Runs only if the View/Edit Feature modal exists on the page.
(function () {
    const editModal   = document.getElementById('editFeatureModal');
    const planSelect  = document.getElementById('f_PlanIds');
    if (!editModal) return; // Not on the Features tab -> bail.

    let choicesInstance = null;

    function initChoices(sel, existing) {
        if (!sel) return null;
        if (window.Choices && !sel.dataset.choices) {
            sel.dataset.choices = '1';
            const inst = new Choices(sel, {
                removeItemButton: true,
                shouldSort: false,
                searchEnabled: true
            });
            if (existing && existing.length) inst.setChoiceByValue(existing);
            return inst;
        }
        return null;
    }

    function setReadonlyFeature(enabled) {
        const ro = !enabled;
        ['f_Name', 'f_Description', 'f_HomeOrder', 'f_HomeTitle', 'f_HomeSummary']
            .forEach(id => {
                const el = document.getElementById(id);
                if (el) el.readOnly = ro;
            });
        document.getElementById('f_Image').disabled = ro; // file input
        if (planSelect) planSelect.disabled = ro;         // multi-select
        const saveBtn = document.getElementById('saveFeatureChangesBtn');
        if (saveBtn) saveBtn.disabled = ro;
    }

    editModal.addEventListener('show.bs.modal', function (ev) {
        const btn = ev.relatedTarget;
        if (!btn) return;

        // Fill fields from data- attributes
        document.getElementById('f_Id').value          = btn.getAttribute('data-id') || '';
        document.getElementById('f_Name').value        = btn.getAttribute('data-name') || '';
        document.getElementById('f_Description').value = btn.getAttribute('data-description') || '';
        document.getElementById('f_HomeOrder').value   = btn.getAttribute('data-homeorder') || '';
        document.getElementById('f_HomeTitle').value   = btn.getAttribute('data-hometitle') || '';
        document.getElementById('f_HomeSummary').value = btn.getAttribute('data-homesummary') || '';

        const imgPath = btn.getAttribute('data-homeimage') || '/images/placeholder.jpg';
        document.getElementById('f_ImagePreview').src = imgPath;
        document.getElementById('f_ExistingImage').value = imgPath;

        // Preselect plans
        const ids = (btn.getAttribute('data-planids') || '')
            .split(',').map(s => s.trim()).filter(Boolean);

        // Rebuild Choices each time so preselection is correct
        if (choicesInstance) { try { choicesInstance.destroy(); } catch (e) {} }
        choicesInstance = initChoices(planSelect, ids);

        // Start in READ-ONLY mode
        setReadonlyFeature(false);
    });

    // Enable editing
    document.getElementById('enableFeatureEditBtn')?.addEventListener('click', function () {
        const disabled = document.getElementById('saveFeatureChangesBtn').disabled;
        setReadonlyFeature(disabled);
    });

    // Image preview
    document.getElementById('f_Image')?.addEventListener('change', function (e) {
        const file = e.target.files?.[0];
        if (!file) return;
        const url = URL.createObjectURL(file);
        document.getElementById('f_ImagePreview').src = url;
    });
})();