(function () {
    var app = angular.module('RequestManagerApp', []);
    var serviceId = 'requestFactory';
    var controllerId = 'requestController';

    // Factory talks to the web server
    app.factory(serviceId, ['$http', factory]);
    function factory($http) {
        function getPage(filterSettings) {
            return $http({
                method: 'POST',
                url: '/datareview/getrequests',
                data: JSON.stringify(filterSettings)
            });
        }
        function review(requestId, approvalStatus, notes) {
            return $http({
                method: 'POST',
                url: '/datareview/review',
                data: JSON.stringify({ requestId: requestId, status: approvalStatus, notes: notes })
            });
        }

        var service = {
            getPage: getPage,
            review: review
        };

        return service;
    }

    app.controller(controllerId, ['$scope', 'requestFactory', controller]);
    function controller($scope, factory) {
        $scope.requestsList = [];
        $scope.selectedItem = null;
        $scope.filterSettings = {
            Submitter: '',
            SubmissionDate: null,
            RequestType: 0,
            HasBeenReviewed: -1,
            HasBeenApproved: -1,
            Page: 1,
            ResultsPerPage: 25
        };
        $scope.totalResults = 0;
        $scope.totalPages = 0;
        $scope.pageNumber = 0;
        $scope.selectedIndex = -1;

        $scope.getDisplayPages = function () {
            let displayedPages = ($scope.totalPages - $scope.pageNumber) + 1 <= 10 ? ($scope.totalPages - $scope.pageNumber) + 1 : 10;
            return new Array(displayedPages);
        };

        $scope.getPage = function (pageNumber) {
            $scope.filterSettings.Page = pageNumber;
            // Display the pre-loader and hide the content
            $('#requests-preloader').addClass('active').show();
            $('#requests-list').hide();
            factory.getPage($scope.filterSettings).then(
                function success(response) {
                    console.log(response.data);
                    if (response.status == 200) {
                        console.log(response.data.TotalResults);
                        console.log(response.data.TotalPages);
                        $scope.requestsList = response.data.Requests;
                        $scope.totalResults = response.data.TotalResults;
                        $scope.totalPages = response.data.TotalPages;
                        $scope.pageNumber = response.data.CurrentPage;
                    } else {
                        console.log('Error: (Status: ' + status + ') ' + data);
                    }
                    // Hide the preloader and display the content
                    $('#requests-preloader').removeClass('active').hide();
                    $('#requests-list').show();
                },
                function error(response) {
                    // Hide the preloader and display the content
                    $('#requests-preloader').removeClass('active').hide();
                    $('#requests-list').show();
                });
        };

        $scope.getRequestTypeString = function (index) {
            let stringValue = 'Unknown';

            if ($scope.requestsList.length >= index) {
                switch ($scope.requestsList[index].RequestType) {
                    case 1:
                        stringValue = 'Add';
                        break;
                    case 2:
                        stringValue = 'Update';
                        break;
                    case 3:
                        stringValue = 'Delete';
                        break;
                }
            } 

            return stringValue;
        };

        $scope.getPage(1);
    }
})();

$(document).ready(function () {
    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: 10,
        today: 'Today',
        clear: 'Clear',
        close: 'OK',
        closeOnSelect: true
    });
    $(document).ready(function () {
        $('select').material_select();
    });
    $('ul.tabs').tabs();
});