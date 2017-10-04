// Angular
(function () {
    var app = angular.module('EditFormApp', []);
    var serviceId = 'EditFormFactory';
    var controllerId = 'EditFormController';
    var relationshipChipsInput = null;

    var activeIndexBreakout = -1;
    var activeGroupArrayBreakout = [];

    app.directive('relationshipListDirective', function () {
        return function (scope, element, attrs) {
            if (scope.$last) {
                styleRelationshipList();
            }
        };
    });

    app.directive('dynamicHtmlDirective', function ($compile, $http) {
        return {
            link: function (scope, element, attrs) {
                $http.get('/edit/getinformation?type=media').then(function (result) {
                    element.replaceWith($compile(result.data)(scope));
                    materializeSetup();
                });
            }
        };
    });

    function styleRelationshipList() {
        // Get the list items
        var collection = $('.relationship-group').children('li');
        // Loop through the group array
        for (var x = 0; x < activeGroupArrayBreakout.length; x++) {
            $(collection[x]).removeClass('light-green red accent-1');
            // If this is the active item
            if (x === activeIndexBreakout) {
                $(collection[x]).addClass('light-green accent-1');
            } else {
                // If the element is invalid,
                if (activeGroupArrayBreakout[x].targetName === null || activeGroupArrayBreakout[x].targetName === '') {
                    $(collection[x]).addClass('red accent-1');
                }
            }
        }
    }

    app.factory(serviceId, ['$http', factory]);
    function factory($http) {

        function getInformation(type) {
            return $http({
                method: 'GET',
                url: '/edit/getinformation?type=' + type,
            });
        }

        function sendInformation(node) {
            return $http({
                method: 'POST',
                url: '/edit/submit',
                data: JSON.stringify({ contentType: node.contentType, data: JSON.stringify(node) })
            });
        }

        var service = {
            getInformation: getInformation,
            sendInformation: sendInformation
        };
        return service;
    }

    app.controller(controllerId, ['$scope', 'EditFormFactory', controller]);
    function controller($scope, factory) {
        $scope.node = new BasicNode();
        $scope.activeGroup = 'Companies';
        $scope.activeIndex = -1;
        $scope.activeGroupArray = $scope.node.relatedCompanies;
        $scope.ignoreNextSelection = false;

        $scope.nodeTypeChanged = function () {
            // Enable the relationship button
            $('#add-relationship-button').removeClass('disabled');
            $('#submission-section').find('button').removeClass('disabled');
            // Set the content type
            if ($scope.node.contentType === 'company') {
                $scope.node = new CompanyNode($scope.node);
            } else if ($scope.node.contentType === 'media') {
                $scope.node = new MediaNode($scope.node);
            } else if ($scope.node.contentType === 'person') {
                $scope.node = new PersonNode($scope.node);
            }

            // Get the additional information
            factory.getInformation($scope.node.contentType).then(
                function success(response) {
                    $('#type-specific-info').empty().append(response.data);
                    $('#hidden-divider').show();
                    $('#type-specific-info').show();
                    materializeSetup();
                },
                function error(response) {
                    console.log(error);
                }
            );

            // Disable the content type
            $('#node-content-type').attr('disabled', 'disabled');
        };

        $scope.submitForm = function () {
            console.log($scope.node);
        };

        $scope.resetForm = function () {
            if (window.confirm('Are you sure you want to clear the form? All data will be lost')) {
                $('#submission-section').find('button').addClass('disabled');
                $('#node-content-type').removeAttr('disabled');
                $('select').material_select();
                $('#type-specific-info').empty();
                $('#hidden-divider').hide();
                $('#type-specific-content').hide();
                $scope.node = new BasicNode($scope.node.id);
                console.log($scope.node);
            }
        };

        $scope.selectTab = function (tab) {
            $scope.activeGroup = tab;
            // Select the active group array
            if (tab === 'Companies') {
                $scope.activeGroupArray = $scope.node.relatedCompanies;
            } else if (tab === 'Media') {
                $scope.activeGroupArray = $scope.node.relatedMedia;
            } else if (tab === 'People') {
                $scope.activeGroupArray = $scope.node.relatedPeople;
            }
            // Update the breakout for the directive
            activeGroupArrayBreakout = $scope.activeGroupArray;
            // Deselect group item
            $scope.selectGroupItem(-1);
        };

        $scope.selectGroupItem = function (index) {
            $scope.activeIndex = $scope.ignoreNextSelection ? -1 : index;
            $scope.ignoreNextSelection = false;
            activeIndexBreakout = $scope.activeIndex;

            if ($scope.activeIndex < 0) {
                // Disable the relationship input fields
                disableRelationshipInputs();
                $('#relationship-chips').material_chip({ data: [] });
                activeClassRemover();
            } else {
                // Enable the relationship input fields
                enableRelationshipInputs();
                // Update the chips
                var chipsData = [];
                for (var i = 0; i < $scope.activeGroupArray[$scope.activeIndex].roles.length; i++) {
                    chipsData.push({ tag: $scope.activeGroupArray[$scope.activeIndex].roles[i] });
                }
                $('#relationship-chips').material_chip({ data: chipsData });
                // Remove the 'active' class if there is no data in the relationship inputs
                if ($scope.activeGroupArray[$scope.activeIndex].roles.length === 0) {
                    $('#relationship-chips-label').removeClass('active');
                } else {
                    $('#relationship-chips-label').addClass('active');
                }
                if ($scope.activeGroupArray[$scope.activeIndex].targetName == null ||
                    $scope.activeGroupArray[$scope.activeIndex].targetName == '') {
                    $('#relationship-name-label').removeClass('active');
                } else {
                    $('#relationship-name-label').addClass('active');
                }
            }
            // Update the styling
            styleRelationshipList();
        };

        $scope.addRelationship = function () {
            // Add to the active group
            $scope.activeGroupArray.push(new Relationship($scope.node.id));
            activeGroupArrayBreakout = $scope.activeGroupArray;
            $scope.selectGroupItem($scope.activeGroupArray.length - 1);
        };

        $scope.removeRelationship = function (index) {
            $scope.ignoreNextSelection = true;
            $scope.activeGroupArray.splice(index, 1);
        };

        $scope.getTargetName = function (index) {    
            return $scope.activeGroupArray[index].targetName != null && $scope.activeGroupArray[index].targetName !== '' ?
                $scope.activeGroupArray[index].targetName : "No name given";
        };

        // Wire up events to handle changes to the relationship chips   
        $('#relationship-chips').on('chip.add', function (e, chip) {
            $scope.activeGroupArray[$scope.activeIndex].roles.push(chip.tag);
        });

        $('#relationship-chips').on('chip.delete', function (e, chip) {
            $scope.activeGroupArray[$scope.activeIndex].roles.splice($scope.activeGroupArray[$scope.activeIndex].roles.indexOf(chip.tag), 1);
        });
    };

    // ===== HELPER FUNCTIONS =====
    function activeClassRemover() {
        // Remove the 'active' class from the input fields
        $('#relationship-name-label').removeClass('active');
        $('#relationship-chips-label').removeClass('active');
    }

    // Disables the relationship entry fields
    function disableRelationshipInputs() {
        $('#relationship-name-entry').attr('disabled', 'disabled');
        $('#relationship-chips').addClass('disabled');
        $(relationshipChipsInput).attr('disabled', 'disabled');
    }

    // Enables the relationship entry fields
    function enableRelationshipInputs() {
        $('#relationship-name-entry').removeAttr('disabled');
        $('#relationship-chips').removeClass('disabled');
        $(relationshipChipsInput).removeAttr('disabled');
    }

    // ===== OTHER FUNCTIONS =====

    function BasicNode(definition) {
        return {
            id: definition ? definition.id : null,
            contentType: definition ? definition.contentType : null,
            commonName: definition ? definition.commonName : null,
            otherNames: definition ? definition.otherNames : null,
            releaseDate: definition ? definition.releaseDate : null,
            relatedLinks: definition ? definition.relatedLinks : [],
            relatedCompanies: definition ? definition.relatedCompanies : [],
            relatedMedia: definition ? definition.relatedMedia : [],
            relatedPeople: definition ? definition.relatedPeople : []
        };
    }

    function CompanyNode(basic) {
        var obj = new BasicNode(basic);
        obj.deathDate = null;

        return obj;
    }

    function MediaNode(basic) {
        var obj = new BasicNode(basic);
        obj.mediaType = null;
        obj.franchiseName = null;
        obj.genres = [];
        //media.platforms = null;
        //media.regionalReleaseDates = null;

        return obj;
    }

    function PersonNode(basic) {
        var obj = new BasicNode(basic);
        obj.givenName = null;
        obj.familyName = null;
        obj.deathDate = null;
        obj.status = null;
        //obj.nationality = null;

        return obj;
    }

    function Relationship(source) {
        return {
            sourceId: source | null,
            targetId: null,
            targetName: null,
            roles: []
        };
    }

    function materializeSetup() {
        $('#type-specific-info').find('.datepicker').pickadate({
            selectMonths: true,
            selectYears: 15,
            closeOnSelect: true
        });
        $('select').material_select();
        $('#type-specific-info').find('.chips').material_chip();
    }

    // Jquery set up
    $(document).ready(function () {
        // Don't allow users to press 'enter' and submit the form
        $(window).keydown(function (event) {
            if (event.keyCode == 13) {
                event.preventDefault();
                return false;
            }
        });

        $('select').material_select();
        $('ul.tabs').tabs();
        $('.chips').material_chip();

        // Get the relationship chips input
        relationshipChipsInput = $('#relationship-chips > input');
        $(relationshipChipsInput).attr('disabled', 'disabled');
    });
})();