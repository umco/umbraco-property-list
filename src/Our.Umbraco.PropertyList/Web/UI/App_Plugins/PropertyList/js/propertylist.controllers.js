angular.module("umbraco").controller("Our.Umbraco.PropertyList.Controllers.PropertyListController", [
    "$scope",
    "Our.Umbraco.PropertyList.Resources.PropertyListResources",
    "umbPropEditorHelper",
    function ($scope, propertyListResource, umbPropEditorHelper) {

        $scope.inited = false;

        var dataTypeGuid = $scope.model.config.dataType;
        var minItems = $scope.model.config.minItems || 0;
        var maxItems = $scope.model.config.maxItems || 0;

        $scope.isConfigured = dataTypeGuid != null;

        if (!$scope.isConfigured) {

            // Model is ready so set inited
            $scope.inited = true;

        } else {

            if (!angular.isObject($scope.model.value))
                $scope.model.value = undefined;

            $scope.model.value = $scope.model.value || {
                dtd: dataTypeGuid,
                values: []
            };

            $scope.prompts = {};

            propertyListResource.getPropertyTypeScaffoldByKey(dataTypeGuid).then(function (propertyType) {

                $scope.propertyType = propertyType;

                var propertyTypeViewPath = umbPropEditorHelper.getViewPath(propertyType.view);

                if (!$scope.model.controls) {
                    $scope.model.controls = [];
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

                    $scope.model.controls.push({
                        alias: $scope.model.alias + "_" + idx,
                        config: propertyTypeConfig,
                        view: propertyTypeViewPath,
                        value: value
                    });
                });

                // Model is ready so set inited
                $scope.inited = true;

            });

        }

        $scope.canAdd = function () {
            return !maxItems || maxItems == 0 || $scope.model.controls.length < maxItems;
        }

        $scope.canDelete = function () {
            return !minItems || minItems == 0 || $scope.model.controls.length > minItems;
        }

        $scope.addContent = function (evt, idx) {

            var control = {
                alias: $scope.model.alias + "_" + idx,
                config: JSON.parse(JSON.stringify($scope.propertyType.config)),
                view: umbPropEditorHelper.getViewPath($scope.propertyType.view),
                value: ""
            };

            $scope.model.controls.splice(idx, 0, control);
            $scope.setDirty();
        }

        $scope.deleteContent = function (evt, idx) {
            $scope.model.controls.splice(idx, 1);
            $scope.setDirty();
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".pl__property-wrapper",
            helper: function () {
                return $('<div class=\"pl__sortable-helper\"><div><i class=\"icon icon-navigation\"></i></div></div>');
            },
            cursorAt: {
                top: 0
            },
            stop: function (e, ui) {
                $scope.setDirty();
            }
        };

        $scope.setDirty = function () {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            var tmpValues = [];

            _.each($scope.model.controls, function (control, idx) {
                tmpValues[idx] = control.value;
            });

            $scope.model.value = {
                dtd: dataTypeGuid,
                values: !_.isEmpty(tmpValues) ? tmpValues : []
            };
        });

        $scope.$on('$destroy', function () {
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
