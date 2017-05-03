angular.module("umbraco.resources").factory("Our.Umbraco.PropertyList.Resources.PropertyListResources",
    function ($q, $http, umbRequestHelper) {
        return {
            getDataTypeByKey: function (key) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/PropertyList/PropertyListApi/GetDataTypeByKey",
                        method: "GET",
                        params: { key: key }
                    }),
                    "Failed to retrieve datatype by key"
                );
            },
            getPropertyTypeScaffoldByKey: function (key) {
                return umbRequestHelper.resourcePromise(
                    $http({
                        url: "/umbraco/backoffice/PropertyList/PropertyListApi/GetPropertyTypeScaffoldByKey",
                        method: "GET",
                        params: { key: key }
                    }),
                    "Failed to retrieve property type scaffold by key"
                );
            }
        };
    });