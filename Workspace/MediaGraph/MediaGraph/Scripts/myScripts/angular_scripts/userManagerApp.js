(function() {
    var app = angular.module('UserManagerApp', []);
    var serviceId = 'userFactory';
    var controllerId = 'userController';

    app.factory(serviceId, ['$http', factory]);
    function factory($http) {
        function getPage(pageNum, numPerPage) {
            return $http({
                method: 'GET',
                url: '/usermanager/getpage?page=' + pageNum + '&numPerPage=' + numPerPage,
            });
        }
        function updateUser(user) {
            return $http({
                method: 'POST',
                url: '/usermanager/updateuser',
                data: JSON.stringify({ "user": user })
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
        $scope.userData = [];
        $scope.pages = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        $scope.pageNum = 1;
        $scope.numPerPage = 25;

        $scope.nextPage = function () {
            factory.getPage($scope.pageNum + 1, $scope.numPerPage).then(
                function success(response) {
                    $scope.pageNum += 1;
                    $scope.userData = [];
                    for (var i = 0; i < response.data.users.length; i++) {
                        $scope.userData.push(response.data[i]);
                    }
                },
                function error(response) {
                    console.log(error);
                }
            );
        };
        $scope.prevPage = function () {
            factory.getPage($scope.pageNum - 1, $scope.numPerPage).then(
                function success(response) {
                    $scope.pageNum -= 1;
                    $scope.userData = [];
                    $scope.userData.push(response.data.users);
                },
                function error(response) {
                    console.log(error);
                }
            );
        };
        $scope.moveToPage = function (pageNum) {
            factory.getPage(pageNum, $scope.numPerPage).then(
                function success(response) {
                    $scope.pageNum = pageNum;
                    $scope.userData = [];
                    $scope.userData.push(response.data.users);
                },
                function error(response) {
                    console.log(response);
                }
            );
        };
        $scope.updateUser = function (index) {
            factory.updateUser($scope.userData[index]);
        };

        $scope.lastPage = function () {
            return false;
        };

        $scope.moveToPage(1);
    }
})();