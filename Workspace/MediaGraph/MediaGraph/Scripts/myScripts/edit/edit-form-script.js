$(document).ready(function () {
    // Don't allow users to press 'enter' and submit the form
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });

    $('select').material_select();
    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: 30,
        today: 'Today',
        clear: 'Clear',
        close: 'Ok',
        closeOnSelect: true
    });
    $('ul.tabs').tabs();
    $('.chips').material_chip();

    // Set up change events
    $('#content-type').on('change', function (event) {
        // Enable or disable the relationship addition buttons
        if (this.value === '') {
            $('#add-relationship-btn').addClass('disabled');
        } else {
            $('#add-relationship-btn').removeClass('disabled');
        }

        // Enable or disable the media tab
        if (this.value === 'media') {
            $('#media-info-tab').removeClass('disabled');
        } else {
            $('#media-info-tab').addClass('disabled');
        }
    });
});

// Angular
(function () {
    var app = angular.module('EditFormApp', []);
    var serviceId = 'EditFormFactory';
    var controllerId = 'EditFormController';
    var relationshipChipsInput = $('#relationship-chips').children('input');

    app.factory(serviceId, ['$http', factory]);
    function factory($http) {

        function getInformation(type) {
            return $http({
                method: 'GET',
                url: '/edit/getinformation?type=' + type,
            });
        }

        var service = {
            getInformation: getInformation
        };
        return service;
    }

    app.controller(controllerId, ['$scope', 'EditFormFactory', controller]);
    function controller($scope, factory) {
        $scope.node = {
            contentType: null,
            commonName: null,
            otherNames: [],
            releaseDate: null,
            relatedCompanies: [],
            relatedMedia: [],
            relatedPeople: []
        };
        $scope.activeGroup = 'companies';
        $scope.activeIndex = -1;

        $scope.nodeTypeChanged = function () {
            if ($scope.node.contentType === 'company') {
                $scope.node.dateOfDeath = null;
            } else if ($scope.node.contentType === 'media') {
                //$scope.node.platforms = [];
                //$scope.node.regionalReleaseDates = {};
            } else if ($scope.node.contentType === 'person') {
                $scope.node.dateOfDeath = null;
                $scope.node.status = null;
            }

            // Get the additional information
            factory.getInformation($scope.node.contentType).then(
                function success(response) {
                    console.log(response.data);
                    $('#type-specific-info').empty().append(response.data);
                    $('select').material_select();
                },
                function error(response) {
                    console.log(error);
                });

            console.log($scope.node);
        };
    }
})();