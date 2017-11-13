var isEditingRelationship = false;
var relationshipValidator;
var relationshipChipSettings = {
    placeholder: "Enter a Role",
    secondaryPlaceholder: "+Role",
    tooltip: {
        delay: 150,
        tooltip: 'Enter a role and press enter.',
        position: 'top'
    }
};

function setupRelationshipForm() {
    // Set up jquery validation on the relationship form
    relationshipValidator = $('#relationship-form').validate({
        rules: {
            targetType: {
                required: true
            },
            targetName: {
                required: true
            }
        },
        messages: {
            targetType: {
                required: "You must specify the type of the related node."
            },
            targetName: {
                required: "You must provide the name of the related node."
            }
        }
    });
    // Set up autocomplete on the target name field
    setupAutocomplete('#relationship-target', false, autocompleteCallback);
    // Autocomplete callback function
    function autocompleteCallback(data) {
        document.forms['relationship-form']['targetId'].value = data.id;
    }
    // Create the chips input
    customChips('#relationship-chips', relationshipChipSettings);

    // Set up the click handler on the add relationship button
    $('#add-relationship-button').on('click', function (event) {
        // If the form is valid,
        if (relationshipValidator.form()) {
            // Perform the action
            var form = document.forms['relationship-form']; // Get the relationship form
            // If the form has a target id or target name, and roles
            if ((form['targetId'].value != null && form['targetId'].value != '') ||
                (form['targetName'].value != null && form['targetName'].value != '')) {
                // Get the roles information
                var roles = getChipData('#relationship-chips');

                // If we are editing a relationship,
                if (isEditingRelationship) {
                    editRelationship({
                        updateId: network.getSelection().nodes[0],
                        targetId: form['targetId'].value,
                        targetName: form['targetName'].value,
                        targetType: form['targetType'].value,
                        roles: roles
                    });
                } else {
                    createRelationship({
                        targetName: form['targetName'].value,
                        targetId: form['targetId'].value,
                        targetType: form['targetType'].value,
                        roles: roles,
                    });
                    clearRelationshipForm(); // Clear the form
                }
            }
        } else {
            // Focus on the invalid element
            relationshipValidator.focusInvalid();
        }
    });
    $('#remove-relationship-button').on('click', function (event) {
        // Pressing the tab button to focus will allow you to click on disabled buttons
        // Make sure that the button is not disabled,
        if (!$(this).hasClass('disabled')) {
            var idToRemove = network.getSelection().nodes[0];
            // Get the edges containing the edge to delete
            var edgesToRemove = edgeData.get({
                filter: function (item) {
                    return item.from === idToRemove;
                }
            });
            // Remove the node and related edges
            nodeData.remove(idToRemove);
            edgeData.remove(edgesToRemove);
            // Enter add mode
            enterAddMode();
        }
    });
}

function enterEditMode(data) {
    var form = document.forms['relationship-form'];
    form['targetId'].value = data.nid;
    form['targetName'].value = data.targetName;
    form['targetType'].value = '' + data.targetType;
    $(form['targetType']).material_select();
    var chipData = makeChips(data.roles);
    // Get the chip values
    customChips('#relationship-chips', relationshipChipSettings, chipData);
    $('#remove-relationship-button').removeClass('disabled');
    Materialize.updateTextFields();
    $('#add-relationship-button').html('<i class="material-icons left">edit</i>Edit');
    // Enable edit mode
    isEditingRelationship = true;
}

function enterAddMode() {
    clearRelationshipForm();
    $('#add-relationship-button').html('<i class="material-icons left">add</i>Add');
    $('#remove-relationship-button').addClass('disabled');
    isEditingRelationship = false;
}

function clearRelationshipForm() {
    relationshipValidator.resetForm();
    var form = document.forms['relationship-form'];
    form['targetId'].value = "";
    form['targetName'].value = "";
    form['targetType'].value = "";
    $(form['targetType']).material_select();
    $('label[for="relationship-target"]').removeClass('active');
    customChips('#relationship-chips', relationshipChipSettings);
}

function createNode(data) {
    // Check if a node already exists
    var existing = findNodeWithId(data.targetId);
    var created = null;
    // If a node does not exist,
    if (data.targetId == "" || existing === null) {
        // Create the node
        created = nodeData.add({
            nid: data.targetId,
            roles: data.roles,
            targetName: data.targetName,
            targetType: data.targetType,
            shape: 'dot',
            mass: 2,
            group: data.targetType,
            label: truncateLabel(data.targetName, 15),
            title: data.targetName
        })[0];
    } else {
        createToast('That node already exists', 2500);
    }

    return created;
}

function createRelationship(data) {
    var createdNodeId = createNode(data);
    // Attempt to create a node for the target node
    if (createdNodeId !== null) {
        // Create the relationship between the two nodes
        edgeData.add({
            from: createdNodeId,
            to: 'source',
            label: truncateLabel(data.roles[0]),
            font: { align: 'top' }
        });
    }
}

function editRelationship(data) {
    // Update the relationship
    nodeData.update({
        id: data.updateId,
        label: truncateLabel(data.targetName, 15),
        title: data.targetName,
        nid: data.targetId,
        targetType: data.targetType,
        targetName: data.targetName,
        roles: data.roles
    });
    // Update the relationship
    var updateEdge = edgeData.get({ filter: function (item) { return item.from === data.updateId; } });
    edgeData.update({
        id: updateEdge[0].id,
        label: truncateLabel(data.roles[0], 15)
    });
    // Clear the selection
    network.unselectAll();
    // Enter add mode
    enterAddMode();
}

function findNodeWithId(id) {
    var existing = nodeData.get({
        filter: function (item) {
            return item.nid == id;
        }
    });
    // Return the first result if one was found
    return existing.length === 0 ? null : existing[0];
}