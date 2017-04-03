# Property List

A repeatable list (of selected datatype) for Umbraco.

## TODO

- [ ] Pre Value Editor
	- [ ] Prevalues
		- [ ] DataType Picker
			- [ ] Only save the ID
		- [ ] Minimum/maximum items
		- **Ideas**
			- [ ] Hide label?
			- [ ] Disable sorting?

- [ ] Value Editor
	- [ ] ContentType preview
	- [ ] HTML view
		- [ ] Check if we can we reuse any Umbraco directives?
		- [ ] Check if any UMCO projects are useful for reuse?
	- [ ] CSS
	- [ ] Angular / JS
		- [ ] Prepare the value-editor
			- [ ] Get the DataType by ID; then get...
				- [ ] config/prevalues
				- [ ] view-path
				- [ ] property alias
			- [ ] Initialize the list values
			- [ ] Set the list values
		- [ ] Render the DataType/property-editor
			- [ ] Repeatable
			- [ ] Addable
			- [ ] Removeable
			- [ ] Sortable
		- [ ] Saving the values
	- [ ] C#
		- [ ] Saving the values
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

