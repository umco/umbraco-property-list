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

            $scope.$broadcast("propertyListFormSubmitting");

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
    "dataTypeHelper",
    "dataTypeResource",
    "entityResource",
    "Our.Umbraco.PropertyList.Resources.PropertyListResources",
    function ($scope, dataTypeHelper, dataTypeResource, entityResource, propertyListResource) {

        var vm = this;

        vm.selectedDataType = null;
        vm.allowAdd = true;
        vm.allowRemove = true;
        vm.allowEdit = true;
        vm.sortable = false;

        vm.add = add;
        vm.edit = edit;
        vm.remove = remove;

        if (!!$scope.model.value) {
            propertyListResource.getDataTypeByKey($scope.model.value).then(function (dataType) {
                vm.selectedDataType = dataType;
                vm.allowAdd = false;
            });
        }

        function add() {
            vm.editorPickerOverlay = {
                property: {},
                view: "views/common/overlays/contenttypeeditor/editorpicker/editorpicker.html",
                show: true
            };

            vm.editorPickerOverlay.submit = function (model) {

                entityResource.getById(model.property.dataTypeId, "DataType").then(function (entity) {

                    $scope.model.value = entity.key;

                    vm.selectedDataType = {
                        id: entity.id,
                        key: entity.key,
                        name: entity.name,
                        icon: model.property.dataTypeIcon,
                        selectedEditor: model.property.editor
                    };

                    vm.allowAdd = false;

                    setDirty();
                });

                vm.editorPickerOverlay.show = false;
                vm.editorPickerOverlay = null;
            };

            vm.editorPickerOverlay.close = function (model) {
                vm.editorPickerOverlay.show = false;
                vm.editorPickerOverlay = null;
            };
        };

        function edit() {
            dataTypeResource.getById(vm.selectedDataType.id).then(function (dataType) {

                vm.editorSettingsOverlay = {
                    title: "Editor settings",
                    view: "views/common/overlays/contenttypeeditor/editorsettings/editorsettings.html",
                    dataType: dataType,
                    show: true
                };

                vm.editorSettingsOverlay.submit = function (model) {
                    var preValues = dataTypeHelper.createPreValueProps(model.dataType.preValues);
                    dataTypeResource.save(model.dataType, preValues, false).then(function (newDataType) {
                        vm.selectedDataType.name = newDataType.name;
                        vm.editorSettingsOverlay.show = false;
                        vm.editorSettingsOverlay = null;
                        setDirty();
                    });
                };

                vm.editorSettingsOverlay.close = function (oldModel) {
                    vm.editorSettingsOverlay.show = false;
                    vm.editorSettingsOverlay = null;
                };

            });
        };

        function remove() {
            $scope.model.value = null;
            vm.selectedDataType = null;
            vm.allowAdd = true;
            setDirty();
        };

        function setDirty() {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

    }]);

angular.module("umbraco.directives").directive("propertyListPropertyEditor", [
    function () {

        var link = function ($scope, $element, $attrs, $ctrl) {

            var unsubscribe = $scope.$on("propertyListFormSubmitting", function (ev, args) {
                $scope.$broadcast("formSubmitting", { scope: $scope });
            });

            $scope.$on("$destroy", function () {
                unsubscribe();
            });
        };

        return {
            require: "^form",
            restrict: "E",
            rep1ace: true,
            link: link,
            template: '<umb-property-editor model="control" />',
            scope: {
                control: "=model"
            }
        };
    }
]);

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
