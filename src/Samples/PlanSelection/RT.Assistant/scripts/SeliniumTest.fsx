// Load the required packages
#r "nuget: Selenium.WebDriver"
#r "nuget: Selenium.Support"

open System
open System.IO
open OpenQA.Selenium
open OpenQA.Selenium.Support
open OpenQA.Selenium.Edge
open OpenQA.Selenium.Support.Extensions

(*
selenium does not work in scripts
unless some libraries are copied to fsi folder
*)

let options = EdgeOptions()

let driver = EdgeDriver(options)

let plansUrl = @"https://<url>"
driver.Url <- plansUrl
Async.Sleep 1500 |> Async.RunSynchronously
let folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/temp"
let path = folder + "/planDocs.html"

let ss = driver.TakeScreenshot()
ss.SaveAsFile(folder + "/ss.png")
File.WriteAllText(path,driver.PageSource)
driver.ExecuteJavaScript("getPlanCardDetail('ESSENT2L')?.contextPrice ?? ''")
driver.ExecuteScript("getPlanCardDetail('ESSENT2L')?.contextPrice ?? ''")
let accept = driver.FindElement(By.Id "onetrust-accept-btn-handler")
if accept <> null then accept.Click()
match driver.FindElement(By.CssSelector "#_15gifts-launcher > div > div._15gifts-t-popup-wrapper.leap-emw5as > div > div > div > div > button._15gifts-engagement_button_secondary.leap-1h1oui4") with
| null -> ()
| s -> s.Click()

"getPlanCardDetail('ESSENT2L')?.contextPrice ?? ''"
