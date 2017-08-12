(function() {
    var app = angular.module('NodeApp', []);
    var serviceId = 'nodeFactory';
    var controllerId = 'nodeController';

    // Factory talks to the web server
    app.factory(serviceId, ['$http', factory]);
    function factory($http) {
        function submit(model) {
            return $http({
                method: 'POST',
                url: '/edit/index',
                data: JSON.stringify({ "model": model })
            });
        }

        var service = {
            submit: submit
        };

        return service;
    }

    // Controller talks to the web page
    app.controller(controllerId, ['$scope', 'nodeFactory', controller]);
    function controller($scope, factory) {
        $scope.data = {
            node: {
                id: null,
                contentType: null,
                commonName: null,
                otherNames: [],
                date: null,
                mediaData: {
                    mediaType: null,
                    franchiseName: null,
                    genres: []
                }
            },
            relationships: {
                companies: [],
                people: [],
                media: []
            }
        };
        $scope.dateString = null;
        $scope.selectedIndex = null;
        $scope.lastSelectedIndex = null;
        $scope.selectedGroup = 'companies';
        $scope.headerText = 'Companies';

        // Set up the chip event that requires access to $scope
        $('#relationship-roles-chips').on('chip.add', function (event, chip) {
            if ($scope.selectedIndex != null) {
                $scope.data.relationships[$scope.selectedGroup][$scope.selectedIndex].roles.push(chip.tag);
            }
        });
        $('#relationship-roles-chips').on('chip.delete', function (event, chip) {
            var index = 0;
            var collection = $scope.data.relationships[$scope.selectedGroup][$scope.selectedIndex].roles;
            while (index < collection.length && collection[index] !== chip.tag) {
                index++;
            }
            // Splice the array
            collection.splice(index, 1);
        });

        // Form Submission
        $scope.submit = function () {
            $scope.collectData();
            factory.submit($scope.data).then(
                function success(response) {
                    // Request accepted - status 200
                    $('#page-content-wrapper').html('<h2>Request Accepted</h2>');
                }, function error(response) {
                    if (response.status == 400) {
                        $('#validationContent').html(response.data);
                    }
                });
        };
        $scope.collectData = function () {
            // Get alternate name chips
            $scope.data.node.otherNames = gatherChips('#alternate-names-chips');
            // Get genre chips if this is a media node
            if ($scope.data.node.contentType === 'media') {
                $scope.data.node.mediaData.genres = gatherChips('#media-genres-chips');
            }
            // Relationship roles are updated as they change

            // Convert the date to ISO format
            if ($scope.dateString != null) {
                $scope.data.node.date = (new Date($scope.dateString).toISOString());
            }
        };
        function gatherChips(chipLocation) {
            var result = [];
            var chipCollection = $(chipLocation).material_chip('data');
            for (var i = 0; i < chipCollection.length; i++) {
                result.push(chipCollection[i].tag);
            }
            return result;
        }

        // Relationships
        $scope.setGroup = function (group) {
            $scope.selectedIndex = null;
            $scope.lastSelectedIndex = null;
            $scope.selectedGroup = group;
            // Update the header             
            $scope.headerText = $scope.selectedGroup.charAt(0).toUpperCase() + $scope.selectedGroup.substring(1);
            // Update entry fields
            updateEntryFields();
        };
        $scope.selectObject = function ($index) {
            $scope.lastSelectedIndex = $scope.selectedIndex;
            $scope.selectedIndex = $index;
            // Update entry fields
            updateEntryFields();
        };
        $scope.addRelationship = function () {
            $scope.data.relationships[$scope.selectedGroup].push({
                relationshipType: $scope.getRelationshipType(),
                otherId: null,
                otherName: null,
                roles: []
            });
            $scope.selectObject($scope.data.relationships[$scope.selectedGroup].length - 1);
        };
        $scope.removeRelationship = function (index) {
            $scope.data.relationships[$scope.selectedGroup].splice(index, 1);
        };

        function updateEntryFields() {
            if ($scope.selectedIndex == null) {
                $('label[for="relationship-target-name"]').removeClass('active');
                $('label[for="relationship-roles-chips"]').removeClass('active');
                // Clear chips
                $('#relationship-roles-chips').material_chip();
            } else if($scope.selectedIndex != $scope.lastSelectedIndex) {
                if ($scope.data.relationships[$scope.selectedGroup].length > 0) {
                    if ($scope.data.relationships[$scope.selectedGroup][$scope.selectedIndex].otherName != null) {
                        $('label[for="relationship-target-name"]').addClass('active');
                    } 
                    if ($scope.data.relationships[$scope.selectedGroup][$scope.selectedIndex].roles.length > 0) {
                        $('label[for="relationship-roles-chips"]').addClass('active');
                    }
                    // Update chips
                    $('#relationship-roles-chips').material_chip({ data: createChips() });
                }
            }
        }
        function createChips() {
            var result = [];
            var roles = $scope.data.relationships[$scope.selectedGroup][$scope.selectedIndex].roles;
            for (var i = 0; i < roles.length; i++) {
                result.push({ tag: roles[i] });
            }
            return result;
        }

        // Helpers
        $scope.getRelationshipType = function () {
            var relType = null;
            if ($scope.data.node.contentType === 2) {
                // We are the target
                relType = $scope.singularizeGroupName() + "-media";
            } else {
                relType = $scope.data.node.contentType + "-" + $scope.singularizeGroupName();
            }

            return relType;
        };
        $scope.singularizeGroupName = function () {
            var result = null;

            if ($scope.selectedGroup === 'companies') {
                result = 'company';
            } else if ($scope.selectedGroup === 'people') {
                result = 'person';
            } else if ($scope.selectedGroup === 'media') {
                result = 'media';
            }

            return result;
        };
        $scope.getRelationshipTitle = function (index) {
            return $scope.data.relationships[$scope.selectedGroup][index].otherName !== null ? $scope.data.relationships[$scope.selectedGroup][index].otherName : 'No Information';
        };
    }

    function logError(response) {
        console.log(response);
    }
})();