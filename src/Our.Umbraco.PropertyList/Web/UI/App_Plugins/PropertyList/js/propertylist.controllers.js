angular.module("umbraco").controller("Our.Umbraco.PropertyList.Controllers.RepeatableDataTypeController",
    function ($scope, contentTypeResource, umbPropEditorHelper) {

        //console.debug("pl", $scope.model.config.dataType, $scope.model.value);

       var dataTypeId = $scope.model.config.dataType;
       var minItems = $scope.model.config.minItems || 0;
       var maxItems = $scope.model.config.maxItems || 0;

        $scope.isConfigured = dataTypeId != null;

        if ($scope.isConfigured) {

            if (!angular.isObject($scope.model.value))
                $scope.model.value = undefined;

            $scope.model.value = $scope.model.value || {
                dtdId: dataTypeId,
                values: []
            };

            $scope.prompts = {};

            contentTypeResource.getPropertyTypeScaffold(dataTypeId).then(function (propertyType) {

                $scope.propertyType = propertyType;

                var item = {
                    config: propertyType.config,
                    view: umbPropEditorHelper.getViewPath(propertyType.view)
                };

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
                    $scope.model.controls.push({
                        alias: $scope.model.alias + "_" + idx,
                        config: item.config,
                        view: item.view,
                        value: value
                    });
                });

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
                config: $scope.propertyType.config,
                view: umbPropEditorHelper.getViewPath($scope.propertyType.view),
                value: ""
            };

            $scope.model.controls.splice(idx, 0, control);
        }

        $scope.deleteContent = function (evt, idx) {
            $scope.model.controls.splice(idx, 1);
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
            //update: function (e, ui) {
            //    _.each($scope.model.controls, function (itm, idx) {
            //        console.debug("sorted", itm, idx)
            //    });
            //}
        };

        var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            var tmpValues = [];

            _.each($scope.model.controls, function (control, idx) {
                tmpValues[idx] = control.value;
            });

            $scope.model.value.values = !_.isEmpty(tmpValues) ? tmpValues : [];
            $scope.model.value.dtdId = dataTypeId;
        });

        $scope.$on('$destroy', function () {
            unsubscribe();
        });

    });

angular.module("umbraco").controller("Our.Umbraco.PropertyList.Controllers.DataTypePickerController",
    function ($scope, contentTypeResource, dataTypeResource, dataTypeHelper) {

        if (!$scope.model.property) {

            $scope.model.property = {};

            if ($scope.model.value) {
                dataTypeResource.getById($scope.model.value).then(function (dataType) {

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

        function setDataTypeId(dataTypeId) {
            $scope.model.value = dataTypeId;
        };

        function openEditorPickerOverlay(property) {

            vm.editorPickerOverlay = {};
            vm.editorPickerOverlay.property = $scope.model.property;
            vm.editorPickerOverlay.view = "views/common/overlays/contenttypeeditor/editorpicker/editorpicker.html";
            vm.editorPickerOverlay.show = true;

            vm.editorPickerOverlay.submit = function (model) {

                setDataTypeId(model.property.dataTypeId);

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

                            setDataTypeId(newDataType.id);

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

    });

