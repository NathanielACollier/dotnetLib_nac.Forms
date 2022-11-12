# nac.Forms

## Description

Provides a quick way to create a GUI application for Linux, macOS, and Windows.  Provides model binding with INotifyPropertyChange using both a dynamic bindable dictionary as the default and typed classes by setting DataContext.		

## Control Examples

+ Textbox
	+ Code
	```c#
	var f = Avalonia.AppBuilder.Configure<App>()
                                .NewForm();
	f.TextFor("txt2", "Type here")
     .TextBoxFor("txt2")
	 .Display();
	```
	+ Result
	+ ![](/assets/Screenshot_8_17_19__12_53_PM.png)
+ Button with click count
	+ Code
	```c#
	var f = Avalonia.AppBuilder.Configure<App>()
                                .NewForm();
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
	var f = Avalonia.AppBuilder.Configure<App>()
                                .NewForm();
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
+ Tabs
	+ Code
	```c#
	var f = Avalonia.AppBuilder.Configure<App>()
							.NewForm();
	
	f.Tabs(t=> {
		t.Header = "Tab 1";
		t.Populate = f => {
			f.Text("Hello from tab 1");
		};
	}, t => {
		t.Header = "Tab 2";
		t.Populate = f => {
			f.Text("Hello from tab 2");
		};
	})
	.Display();
	```
	+ Result
	+ ![](/assets/2021-05-31_13-58.png)
