angular.module("umbraco").controller("Our.Umbraco.PropertyList.Controllers.PropertyListController", [
    "$scope",
    "Our.Umbraco.PropertyList.Resources.PropertyListResources",
    "umbPropEditorHelper",
    function ($scope, propertyListResource, umbPropEditorHelper) {

        var dataTypeGuid = $scope.model.config.dataType;
        var minItems = $scope.model.config.minItems || 0;
        var maxItems = $scope.model.config.maxItems || 0;
        var propertyType = null; // this will be set in the `getPropertyTypeScaffoldByKey` callback

        var vm = this;

        vm.sortableOptions = {
            axis: "y",
            containment: "parent",
            cursor: "move",
            handle: ".list-view-layout__sort-handle",
            opacity: 0.7,
            scroll: true,
            tolerance: "pointer",
            stop: function (e, ui) {
                setDirty();
            }
        };

        vm.canAdd = canAdd;
        vm.canDelete = canDelete;
        vm.showPrompt = showPrompt;
        vm.hidePrompt = hidePrompt;
        vm.addContent = addContent;
        vm.deleteContent = deleteContent;


        if (!angular.isObject($scope.model.value))
            $scope.model.value = undefined;

        $scope.model.value = $scope.model.value || {
            dtd: dataTypeGuid,
            values: []
        };

        propertyListResource.getPropertyTypeScaffoldByKey(dataTypeGuid).then(function (scaffold) {

            propertyType = scaffold;

            var propertyTypeViewPath = umbPropEditorHelper.getViewPath(propertyType.view);

            if (!vm.controls) {
                vm.controls = [];
            }

            if (!$scope.model.value.values) {
                $scope.model.value.values = [];
            }

            // Enforce min items
            if ($scope.model.value.values.length < minItems) {
                for (var i = $scope.model.value.values.length; i < minItems; i++) {
                    $scope.addContent(null, i);
                }
            }

            _.each($scope.model.value.values, function (value, idx) {

                // NOTE: Must be a copy of the config, not the same object reference.
                // Otherwise any config modifications made by the editor will apply to following editors.
                var propertyTypeConfig = JSON.parse(JSON.stringify(propertyType.config));

                vm.controls.push({
                    alias: $scope.model.alias + "_" + idx,
                    config: propertyTypeConfig,
                    view: propertyTypeViewPath,
                    value: value
                });
            });

        });

        function canAdd() {
            return !maxItems || maxItems === "0" || vm.length < maxItems;
        }

        function canDelete() {
            return !minItems || minItems === "0" || vm.length > minItems;
        }

        function showPrompt(control) {
            control.deletePrompt = true;
        }

        function hidePrompt(control) {
            control.deletePrompt = false;
        }

        function addContent() {

            var control = {
                alias: $scope.model.alias + "_" + (vm.controls.length + 1),
                config: JSON.parse(JSON.stringify(propertyType.config)),
                view: umbPropEditorHelper.getViewPath(propertyType.view),
                value: ""
            };

            vm.controls.push(control);
            setDirty();
        }

        function deleteContent(idx) {
            vm.controls.splice(idx, 1);
            setDirty();
        }

        function setDirty() {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {

            var tmpValue = {
                dtd: dataTypeGuid,
                values: []
            };

            _.each(vm.controls, function (control, idx) {
                tmpValue.values.push(control.value);
            });

            $scope.model.value = tmpValue;
        });

        $scope.$on("$destroy", function () {
            unsubscribe();
        });

    }]);

angular.module("umbraco").controller("Our.Umbraco.PropertyList.Controllers.DataTypePickerController", [
    "$scope",
    "contentTypeResource",
    "dataTypeHelper",
    "dataTypeResource",
    "entityResource",
    "Our.Umbraco.PropertyList.Resources.PropertyListResources",
    function ($scope, contentTypeResource, dataTypeHelper, dataTypeResource, entityResource, propertyListResource) {

        if (!$scope.model.property) {

            $scope.model.property = {};

            if ($scope.model.value) {
                propertyListResource.getDataTypeByKey($scope.model.value).then(function (dataType) {
                    // update editor
                    $scope.model.property.editor = dataType.selectedEditor;
                    $scope.model.property.dataTypeId = dataType.id;
                    $scope.model.property.dataTypeIcon = dataType.icon;
                    $scope.model.property.dataTypeName = dataType.name;
                });
            }
        }

        var vm = this;

        vm.openEditorPickerOverlay = openEditorPickerOverlay;
        vm.openEditorSettingsOverlay = openEditorSettingsOverlay;

        function setModelValue(dataTypeId) {
            entityResource.getById(dataTypeId, "DataType").then(function (entity) {
                $scope.model.value = entity.key;
            });
        };

        function openEditorPickerOverlay(property) {

            vm.editorPickerOverlay = {};
            vm.editorPickerOverlay.property = $scope.model.property;
            vm.editorPickerOverlay.view = "views/common/overlays/contenttypeeditor/editorpicker/editorpicker.html";
            vm.editorPickerOverlay.show = true;

            vm.editorPickerOverlay.submit = function (model) {

                setModelValue(model.property.dataTypeId);

                vm.editorPickerOverlay.show = false;
                vm.editorPickerOverlay = null;
            };

            vm.editorPickerOverlay.close = function (model) {
                vm.editorPickerOverlay.show = false;
                vm.editorPickerOverlay = null;
            };

        }

        function openEditorSettingsOverlay(property) {

            // get data type
            dataTypeResource.getById(property.dataTypeId).then(function (dataType) {

                vm.editorSettingsOverlay = {};
                vm.editorSettingsOverlay.title = "Editor settings";
                vm.editorSettingsOverlay.view = "views/common/overlays/contenttypeeditor/editorsettings/editorsettings.html";
                vm.editorSettingsOverlay.dataType = dataType;
                vm.editorSettingsOverlay.show = true;

                vm.editorSettingsOverlay.submit = function (model) {

                    var preValues = dataTypeHelper.createPreValueProps(model.dataType.preValues);

                    dataTypeResource.save(model.dataType, preValues, false).then(function (newDataType) {

                        contentTypeResource.getPropertyTypeScaffold(newDataType.id).then(function (propertyType) {

                            setModelValue(newDataType.id);

                            // update editor
                            property.config = propertyType.config;
                            property.editor = propertyType.editor;
                            property.view = propertyType.view;
                            property.dataTypeId = newDataType.id;
                            property.dataTypeIcon = newDataType.icon;
                            property.dataTypeName = newDataType.name;

                            vm.editorSettingsOverlay.show = false;
                            vm.editorSettingsOverlay = null;

                        });

                    });

                };

                vm.editorSettingsOverlay.close = function (oldModel) {
                    vm.editorSettingsOverlay.show = false;
                    vm.editorSettingsOverlay = null;
                };

            });

        }

    }]);

angular.module("umbraco.resources").factory("Our.Umbraco.PropertyList.Resources.PropertyListResources", [
    "$http",
    "umbRequestHelper",
    function ($http, umbRequestHelper) {
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
    }
]);
