# Avalonia NCForms

## Building
+ Avalonia
	+ https://github.com/AvaloniaUI/Avalonia
	+ Use nuget packages from nuget `Avalonia.Desktop`
				
## Control Examples

+ Textbox
	+ Code
	```c#
	var f = new dotnetCoreAvaloniaNCForms.Form();
	f.TextFor("txt2", "Type here")
     .TextBoxFor("txt2")
	 .Display();
	```
	+ Result
	+ ![](/assets/Screenshot_8_17_19__12_53_PM.png)
+ Button with click count
	+ Code
	```c#
	var f = new dotnetCoreAvaloniaNCForms.Form();
	f.TextFor("txt1", "When you click button I'll change to count!")
	.Button("Click Me!", arg =>
	{
		var current = child.Model.GetOrDefault<int>("txt1", 0);
		++current;
		child.Model["txt1"] = current;
	})
	.Display();
	```
	+ Result
	+ ![](/assets/201908170102PM.png)
+ Horizontal Group
	+ Code
	```c#
	var f = new dotnetCoreAvaloniaNCForms.Form();
	f.HorizontalGroup(hori =>
	{
		hori.Text("Click Count: ")
			.TextBoxFor("clickCount")
			.Button("Click Me!", arg =>
			{
				var current = child.Model.GetOrDefault<int>("clickCount", 0);
				++current;
				hori.Model["clickCount"] = current;
			});
	})
	.Display();
	```
	+ Result
	+ ![](/assets/Screenshot_8_17_19__2_36_PM.png)
