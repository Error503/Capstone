(function() {
    var app = angular.module('UserManagerApp', []);
    var serviceId = 'userFactory';
    var controllerId = 'UserManagerController';

    app.directive('userListDirective', function () {
        return function (scope, element, attrs) {
            element.find('select').val(scope.user.Role);
            if (scope.$last) {
                $('select').material_select();
            }
        };
    });

    app.factory(serviceId, ['$http', factory]);
    function factory($http) {
        function getPage(filterSettings) {
            return $http({
                method: 'GET',
                url: '/admintools/getuserpage?username=' + (filterSettings.userName != null ? filterSettings.userName : "") +
                '&email=' + (filterSettings.userEmail != null ? filterSettings.userEmail : "") + '&page=' + filterSettings.page + '&resultsPerPage=' + filterSettings.resultsPerPage,
            });
        }
        function updateUser(userId, role) {
            return $http({
                method: 'POST',
                url: '/admintools/update',
                data: JSON.stringify({ id: userId, role: role })
            });
        }

        var service = {
            getPage: getPage,
            updateUser: updateUser
        };

        return service;
    }

    app.controller(controllerId, ['$scope', 'userFactory', controller]);
    function controller($scope, factory) {
        $scope.filter = {
            userName: null,
            userEmail: null,
            userRole: null,
            resultsPerPage: 25,
            page: 1
        };
        $scope.totalPages = 0;
        $scope.currentPage = 0;
        $scope.users = [];

        $scope.applyFilter = function () {
            $scope.currentPage = 0;
            $scope.totalPages = 0;
            $scope.filter.resultsPerPage = Number.parseInt($('#resultsPerPage').val());
            $scope.getPage(1);
        };

        $scope.getPage = function (page) {
            if (page == $scope.currentPage || (page > $scope.totalPages && $scope.totalPages > 0) || page <= 0) {
                return;
            }
            $scope.filter.page = page;
            // Display the preloader
            $('#preloader').addClass('active').show();
            $('#user-list').hide();

            // Run the request
            factory.getPage($scope.filter).then(
                function success(response) {
                    console.log(response.data);
                    // Update the data
                    $scope.currentPage = page;
                    $scope.totalPages = response.data.PageCount; 
                    $scope.users = response.data.Users;
                    // Hide the preloader
                    $('#preloader').removeClass('active').hide();
                    $('#user-list').show();
                },
                function error(response) {
                    console.error(response);
                    // Do not hide the pre loader on error
                }
            );
        };

        $scope.updateUser = function (index) {
            factory.updateUser($scope.users[index], $scope.users[index].role).then(
                function success(response) {
                    // Display a toast message using materialize
                },
                function error(response) {
                    console.error(response);
                }
            );
        };

        // Get the first page
        $scope.getPage(1);

        $(document).ready(function () {
            $('#page-selection-box').on('change', function (event) {
                $scope.getPage(this.value);
            });
            $('select').material_select();
        });
    }
})();