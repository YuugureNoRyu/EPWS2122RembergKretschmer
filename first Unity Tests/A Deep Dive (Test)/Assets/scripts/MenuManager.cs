using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
   public Panel currentPanel=null;
   private List<Panel> PanelHistory= new List<Panel>();

private void Start()
{SetupPanels();}

private void SetupPanels()
{
    Panel[] panels = GetComponentsInChildren<Panel>();

    foreach(Panel panel in panels)
    panel.Setup(this);

    currentPanel.Show();
}

public void gotoPrevious()
{
if(PanelHistory.Count==0)
return;
int lastIndex=PanelHistory.Count -1;
SetCurrent(PanelHistory[lastIndex]);
PanelHistory.RemoveAt(lastIndex);
}
public void SetCuttentWithHistory(Panel newPanel)
{
PanelHistory.Add(currentPanel);
SetCurrent(newPanel);
}
private void SetCurrent(Panel newPanel)
{
currentPanel.Hide();
currentPanel=newPanel;
currentPanel.Show();
}
}
