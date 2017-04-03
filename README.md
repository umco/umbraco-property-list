# Property List

A repeatable list (of selected datatype) for Umbraco.

## TODO

- [x] Pre Value Editor
	- [x] Prevalues
		- [x] DataType Picker
			- [x] Only save the ID
		- [x] Minimum/maximum items
		- **Ideas**
			- [ ] Hide label?
			- [ ] Disable sorting?

- [ ] Value Editor
	- [x] ContentType preview
	- [x] HTML view
		- [x] Check if we can we reuse any Umbraco directives?
		- [x] Check if any UMCO projects are useful for reuse?
	- [x] CSS
	- [ ] Angular / JS
		- [x] Prepare the value-editor
			- [x] Get the DataType by ID; then get...
				- [x] config/prevalues
				- [x] view-path
				- [x] property alias
			- [x] Initialize the list values
			- [x] Set the list values
		- [x] Render the DataType/property-editor
			- [x] Repeatable
			- [x] Addable
			- [x] Removeable
			- [x] Sortable
		- [x] Saving the values
			- [ ] PropertyValueEditor
				- [ ] ConvertDbToString
				- [ ] ConvertDbToEditor
				- [ ] ConvertEditorToDb
	- [ ] Browser testing (Chrome, Firefox, IE/Edge)
	
- [ ] PropertyValueConverter
	- [ ] Get target DataType definition
	- [ ] Create dummy PropertyType (in order to run the target property-editor's value-converter)
	- [ ] Return as IEnumerable of that type
	- [ ] Investigate ModelsBuilder support - think it's to provide the return type

- [ ] Packaging
	- [ ] MSBuild script
		- [ ] Umbraco package
		- [ ] NuGet package
	- [ ] AppVeyor

- [ ] Courier data-resolver
	- [ ] Pre Value Editor
		- [ ] Convert the DataType ID to the GUID
	- [ ] Value Editor
		- [ ] Convert the DataType ID to the GUID
		- [ ] Processing all list item DataTypes

