(function () {
    var app = angular.module('RequestManagerApp', []);
    var serviceId = 'requestFactory';
    var controllerId = 'RequestManagerController';

    app.factory(serviceId, ['$http', factory]);
    function factory($http) {
        function getPage(filterSettings) {
            return $http({
                method: 'GET',
                url: '/admintools/requestpage?requestType=' + filterSettings.requestType + '&nodeName=' + filterSettings.nodeName +
                '&page=' + filterSettings.page + '&resultsPerPage=' + filterSettings.resultsPerPage
            });
        }

        var service = {
            getPage: getPage
        };

        return service;
    }

    app.controller(controllerId, ['$scope', 'requestFactory', controller]);
    function controller($scope, factory) {
        $scope.filter = {
            requestType: 0,
            nodeName: null,
            page: 1,
            resultsPerPage: 25
        };
        $scope.requestList = [];
        $scope.currentPage = 0;
        $scope.totalPages = 0;

        $scope.applyFilter = function () {
            $scope.currentPage = 1;
            $scope.resultsPerPage = Number.parseInt($('#resultsPerPage').val());
            $scope.getPage(1);
        };

        $scope.getPage = function (page) {
            if (page == $scope.currentPage || (page > $scope.totalPages && $scope.totalPages > 0) || page <= 0) {
                return;
            }
            $scope.filter.page = page;

            $('#request-list').hide();
            $('#preloader').addClass('active').show();

            factory.getPage($scope.filter).then(
                function success(response) {
                    $scope.currentPage = page;
                    $scope.totalPages = response.data.totalPages;
                    $scope.requestList = response.data.requests;
                    console.log($scope.requestList);

                    for (var i = 0; i < $scope.requestList.length; i++) {
                        $scope.requestList[i].NodeData = JSON.parse($scope.requestList[i].NodeData);
                    }

                    $('#preloader').removeClass('active').hide();
                    $('#request-list').show();
                },
                function error(response) {
                    console.log(response);
                }
            );
        };

        $scope.getPage(1);
    }
})();