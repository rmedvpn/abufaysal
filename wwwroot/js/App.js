
function AjaxAction(theAction, value, param1, param2,param3) {
        param1 = param1 || "";
        param2 = param2 || "";
        param3 = param3 || "";
        var theHandler = "Scripts/AjaxActions";

        var formData = new FormData();

                switch (theAction) {

                    case "SaveMenu":
                        var form = document.getElementById(param1);
                        formData = new FormData(form);
                        formData.append("menu_id", value);
                    break;


                    default:
                    break;
                }


        formData.append("theAction", theAction);
        formData.append("value", value);
        formData.append("param1", param1);
        formData.append("param2", param2);
        formData.append("param3", param3);


        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                var theRes = this.responseText;
                switch (theAction) {

                    case "NewMenu":
                        Notify(this.responseText, 1);
                        sbload('MenuEditor', 'MenuEditor');
                        break;



                    default:
                        Notify(theRes, 1);
                        break;
                }

            } console.log(this.responseText);
        };
        xmlhttp.open("POST", theHandler, true);

        xmlhttp.send(formData);
    }

function ControlButton() {
    console.log(currentlyOpenSidebar);

    try {
        clearInterval(statUpdateTimeout);
        statUpdateTimeout = 0;

    } catch (error) {
        console.log(error);
    }


    if (callerSidebar != "") {
        controller.open(callerSidebar);
        currentlyOpenSidebar = callerSidebar;
        switch (sbRefresh) {
            case "WhatEver":
                break;
        }
        callerSidebar = "";
    }
    else {
        switch (currentlyOpenSidebar) {
            case "MahWorld":
                controller.close("MahWorld");
                currentlyOpenSidebar = "";
                console.log('sdf');
                break;
            case "MainSb":
                controller.close("MainSb");
                currentlyOpenSidebar = "";
                break;
            case "SubMenu":
                controller.open("MainSb");
                currentlyOpenSidebar = "MainSb";
                clearContent('SubMenuContent');
            case "SubMenuDialog":
                controller.open("SubMenu");
                currentlyOpenSidebar = "SubMenu";
                clearContent('SubMenuDialogContent');



                break;

            case "":
                // controller.open('msngr');
                //   currentlyOpenSidebar = 'msngr';
                break;



        }
    }



}
