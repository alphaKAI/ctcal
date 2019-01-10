module DataFetch
open System.IO
open System.Reflection
open OpenQA.Selenium.Chrome
open OpenQA.Selenium

open ClassPrinter
open System
open OpenQA.Selenium.Support.UI

type TwinsCredential = {
  id: string;
  pw: string;
}
let twins_url = "https://twins.tsukuba.ac.jp/campusweb/campusportal.do"

let parse_class_entry (text : string) =
  let splitted = String.filter (fun c -> c <> '\r') text |> String.split '\n'
  if Array.length splitted = 1 then
    Unregistered
  else
    Registered {
      id=splitted.[0]
      name=splitted.[1]
      teacher=splitted.[2]
    }

let data_fetch credential =
  let options = new ChromeOptions()
  options.AddArgument("--headless")
  let chrome = new ChromeDriver(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options)
  chrome.Url <- twins_url

  let id_form = chrome.FindElementByXPath "//*[@id=\"LoginFormSlim\"]/tbody/tr/td[3]/input"
  id_form.SendKeys credential.id
  let pw_form = chrome.FindElementByXPath "//*[@id=\"LoginFormSlim\"]/tbody/tr/td[5]/input"
  pw_form.SendKeys credential.pw
  pw_form.SendKeys Keys.Enter

  //Threading.Thread.Sleep 800
  WebDriverWait(chrome, TimeSpan.FromSeconds(5.)).Until(fun driver ->  driver.FindElement (By.XPath "//*[@id=\"portal-system-menu\"]")) |> ignore

  chrome.Url <- "https://twins.tsukuba.ac.jp/campusweb/campusportal.do?page=main&tabId=rs"

  let iframe = chrome.FindElementByXPath "//*[@id=\"main-frame-if\"]"
  chrome.SwitchTo().Frame(iframe) |> ignore

  let ret =
    MyUtil.range 2 7 |> List.map (fun i ->
      MyUtil.range 2 6 |> List.map (fun j ->
        let xpath = sprintf "/html/body/table[2]/tbody/tr[2]/td/table/tbody/tr[%d]/td[%d]" i j
        let elem = chrome.FindElementByXPath xpath
        parse_class_entry elem.Text))

  chrome.Quit ()

  ret