var currentlyOpenSidebar = "";
var currentlyOpenSubSidebar = "";
var callerSidebar = "";
var AppNotifyTimer = "";
var AppNotifyStopToken = "";
var boolFlag = false;

function showExternalMenu(caller_id, menu_name) {
    caller_id = caller_id || "";
    if (caller_id == "") {
        controller.close(menu_name);
        currentlyOpenSidebar = '';
    }
    else {
        controller.open(caller_id);
        currentlyOpenSidebar = caller_id;

    }
}

function ControlButton() {
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

function AppNotify(n_id, ret_containter, stop_token, ref_rate) {
    n_id = n_id || "";
    ret_containter = ret_containter || "";
    stop_token = stop_token || "";
    ref_rate = ref_rate || 1000;
    AppNotifyStopToken = "start";
    AppNotifyTimer = setInterval(function () { AjaxUpdate("APP_NOTIFY", ret_containter, '', n_id, stop_token); }, ref_rate);

}
function StopAppNotify(stop_token) {
    stop_token = stop_token || "";
    clearInterval(AppNotifyTimer);
}

function UserTrack() {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            var theResponse = this.responseText;
            if (theResponse.substring(0, 3) == '!!!') {

                //Do something about logout?
                //alert('bye');
                window.location.href = "/Login";
            }

            //  console.log('UserTrack ');


        }
    };
    xmlhttp.open("GET", "Scripts/UserMonitor", true);

    xmlhttp.send();
}


/////////////
function AjaxActions(field, value, loader_element, param1, param2, param3, param4) {
    loader_element = loader_element || "";
    param1 = param1 || "";
    param2 = param2 || "";
    param3 = param3 || "";
    param4 = param4 || "";
    var theHandler = "/Scripts/AjaxActions";
    console.log(theHandler);

    var formData = new FormData();

    if (document.getElementById(loader_element)) {
        showElement(loader_element);
    }
    switch (field) {

        case "RegisterProspect":
        case "UserSignUp":
        case "UpdateSettings":
        case "AddToCart":
        case "ReviewOrder":
        case "PlaceOrder":
        case "CheckRegistrationCode":
            var form = document.getElementById(param1);
            formData = new FormData(form);
            break;
    }

    formData.append("theAction", field);
    formData.append("field", field);
    formData.append("value", value);
    formData.append("param1", param1);
    formData.append("param2", param2);
    formData.append("param3", param3);
    formData.append("param4", param4);

    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            if (document.getElementById(loader_element)) {
                hideElement(loader_element);
            }
            var theRes = this.responseText;

            switch (field) {
                case "UpdateSettings":
                    if (!theRes.startsWith("!$!")) {
                        sbload('MainSb', 'prsettings');
                    }

                    Notify(theRes.replace("!$!", ""), 1);
                    break;

                case "RegisterProspect":
                    if (!theRes.startsWith("!$!")) {
                        showElement('prospectJoinValidationError');
                        console.log('sdfasdfasd123');

                        window.location.href = "Register_thankyou";

                    }
                    else {
                        document.getElementById('prospectJoinValidationError').innerHTML = theRes.replace("!$!", "");
                        showElement('prospectJoinValidationError');
                        hideElement('prospectJoinLoader');
                        showElement('prospectJoinActions');
                    }

                    break;

                case "CheckRegistrationCode":


                    if (theRes.startsWith("!$!")) {
                        document.getElementById('PreApprovalErrorDiv').innerHTML = theRes.replace("!$!", "");
                        showElement('PreApprovalErrorDiv');
                        console.log('PreApprovalErrorDiv');
                    } else {

                        // AjaxUpdate('RegCodeSignup', 'MainRegistrationDiv', 'MainLoader', theRes);
                        sbload('MainSb', 'Caller?p1=RegCodeSignup&code=' + theRes);
                    
                        console.log('sbload BottomSubMenu');
                        //document.getElementById('PreApprovalErrorDiv').innerHTML = theRes.replace("!$!", "");
                    }
                    break;

                case "UserSignUp":
                    if (!theRes.startsWith("!$!")) {

                        window.location.href = "/";

                    }
                    else {
                        document.getElementById('SignUpErrorDiv').innerHTML = theRes.replace("!$!", "");
                        showElement('SignUpErrorDiv');

                    }

                    break;

                case "AddToCart":
                case "ClearCart":

                    PageLoad("ORDERS");

                    break;

                case "CartAction":
                    if (!theRes.startsWith("!$!")) {
                        if (value == 'REMOVE') {
                            PageLoad("ORDERS");
                        }
                        else {
                            AjaxUpdate('Cart', 'MainBodyContent', 'MainLoader', 'CalcItemCostForm');
                        }
                        

                    }

                    if (!theRes.startsWith("!!!")) {
                        Notify(theRes.replace("!$!", ""), 1);
                        console.log('theRes ' + theRes);
                    }

                    break;

                case "ReviewOrder":
                    if (theRes.startsWith("!$!")) {
                        Notify(theRes.replace("!$!", ""), 1);


                    }
                    else {
                        sbload('SubMenu', 'OrderReview', '', 'CalcItemCostForm');

                    }

                    break;


                case "PlaceOrder":
                    if (!theRes.startsWith("!$!")) {
                        sbload('SubMenu', 'OrderThankYou', '', 'CalcItemCostForm');

                    }
                    else {
                        Notify(theRes.replace("!$!", ""), 1);
                    }


                    break;




            }


            // Notify(theRes.replace("!$!", ""), 1);

        } console.log(this.responseText);

    };
    xmlhttp.open("POST", theHandler, true);
    xmlhttp.send(formData);
}


function CalcItemCost() {
    if (
        document.getElementById('quan').value.trim() !== "" &&
        document.getElementById('strain').value.trim() !== "" &&
        document.getElementById('paper').value.trim() !== "" &&
        document.getElementById('mixture').value.trim() !== "" &&
        document.getElementById('MixturePotency').value.trim() !== ""
    ) {
        AjaxUpdate('CalcItemCost', 'ItemCostReturnContainer', 'MainLoader', 'CalcItemCostForm');
    }

}

function AjaxUpdate(theAction, return_container, loaderElement, param1, param2, param3, param4) {
    loaderElement = loaderElement || "";
    param1 = param1 || "";
    param2 = param2 || "";
    param3 = param3 || "";
    param4 = param4 || "";




    var handlerUrl = "";
    var formData = new FormData();

    handlerUrl = "Scripts/AjaxHandler";
    if (loaderElement != "") {
        showElement(loaderElement);
    }

    switch (theAction) {
        case "URL":
            handlerUrl = param1;
            break;

        case "CalcItemCost":
            var form = document.getElementById(param1);
            if (form) {
                formData = new FormData(form);
            }

            break;

        default:
            break;


    }

    formData.append("theAction", theAction);
    formData.append("param1", param1);
    formData.append("param2", param2);
    formData.append("param3", param3);
    formData.append("param4", param4);

    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {

            console.log(theAction + ":" + this.responseText);



            switch (theAction) {
                case "RepListSettings":

                    break;


                default:
                    break;
            }


            if (loaderElement != "") {
                hideElement(loaderElement);
            }

            document.getElementById(return_container).innerHTML = this.responseText;


        }

    };

    xmlhttp.open("POST", handlerUrl, true);
    xmlhttp.send(formData);


}

function intBehave(theAction, ActionParam) {

    switch (theAction) {

        case "settingsSelector":
            if (ActionParam == 'INFO') {
                hideElement('newsPref');
                showElement('infoEdit');
                showElement('prospectJoinActions');
            }
            if (ActionParam == 'ADD' || ActionParam == 'REMOVE') {
                hideElement('infoEdit');
                showElement('newsPref');
                showElement('prospectJoinActions');
            }
            if (ActionParam == '') {
                hideElement('newsPref');
                hideElement('infoEdit');
                hideElement('prospectJoinActions');
            }


            break;

        default:
            break;

    }

}

function MiscActions(theAction, param1) {
    theAction = theAction || "";
    param1 = param1 || "";
    console.log(theAction);
    switch (theAction) {
        case "CalcQuanLaborCharge":

            let labor_cost_fld = document.getElementById('quan_labor_cost_' + param1);
            let quan_multiplier_fld = document.getElementById('quan_multiplier_' + param1);
            let labor_charge_fld = document.getElementById('quan_labor_charge_' + param1);
            let labor_cost = 0; try { labor_cost = labor_cost_fld.value; } catch (e) { }
            let quan_multiplier = 0; try { quan_multiplier = quan_multiplier_fld.value; } catch (e) { }
            let labor_charge = 0;
            if (quan_multiplier > 0 && labor_cost > 0) {
                labor_charge = labor_cost * quan_multiplier;
            }
            //labor_charge_fld.value = labor_charge;
            labor_charge_fld.value = labor_charge.toFixed(2);


            break;
        case "CalcStrainGramCharge":

            let gram_cost_fld = document.getElementById('strain_gram_cost_' + param1);
            console.log('gram_cost_fld ', gram_cost_fld);
            let strain_multiplier_fld = document.getElementById('strain_multiplier_' + param1);
            console.log('strain_multiplier_fld ', strain_multiplier_fld);

            let gram_charge_fld = document.getElementById('strain_gram_charge_' + param1);
            console.log('gram_charge_fld ', gram_charge_fld);

            let gram_cost = 0; try { gram_cost = gram_cost_fld.value; } catch (e) { }
            console.log('gram_cost ', gram_cost);

            let strain_multiplier = 0; try { strain_multiplier = strain_multiplier_fld.value; } catch (e) { }
            console.log('strain_multiplier ', strain_multiplier);

            let gram_charge = 0;
            if (strain_multiplier > 0 && gram_cost > 0) {
                gram_charge = gram_cost * strain_multiplier;
            }
            //labor_charge_fld.value = labor_charge;
            gram_charge_fld.value = gram_charge.toFixed(2);

            console.log('gram_charge ', gram_charge);
            break;

        case "CalcmixtureGramCharge":

            let mixture_gram_cost_fld = document.getElementById('mixture_gram_cost_' + param1);
            console.log('mixture_gram_cost_fld ', mixture_gram_cost_fld);
            let mixture_multiplier_fld = document.getElementById('mixture_multiplier_' + param1);
            console.log('mixture_multiplier_fld ', mixture_multiplier_fld);

            let mixture_gram_charge_fld = document.getElementById('mixture_gram_charge_' + param1);
            console.log('mixture_gram_charge_fld ', mixture_gram_charge_fld);

            let mixture_gram_cost = 0; try { mixture_gram_cost = mixture_gram_cost_fld.value; } catch (e) { }
            console.log('mixture_gram_cost ', mixture_gram_cost);

            let mixture_multiplier = 0; try { mixture_multiplier = mixture_multiplier_fld.value; } catch (e) { }
            console.log('mixture_multiplier ', mixture_multiplier);

            let mixture_gram_charge = 0;
            if (mixture_multiplier > 0 && mixture_gram_cost > 0) {
                mixture_gram_charge = mixture_gram_cost * mixture_multiplier;
            }
            //labor_charge_fld.value = labor_charge;
            mixture_gram_charge_fld.value = mixture_gram_charge.toFixed(2);

            console.log('mixture_gram_charge ', mixture_gram_charge);
            break;


        case "":
            // controller.open('msngr');
            //   currentlyOpenSidebar = 'msngr';
            break;



    }



}

function PageLoad(thePage) {
    thePage = thePage || "";
    console.log("start");
    console.log(thePage);
    switch (thePage) {
        case "CHECKOUT":
            console.log('CLICK PAGELOAD');
            AjaxUpdate("Checkout", "MainBodyContent", "MainLoader");
            break;
        case "ORDER":
        case "ORDERS":
            AjaxUpdate("Order","MainBodyContent","MainLoader");
            break;

        case "CART":
            AjaxUpdate("Cart","MainBodyContent","MainLoader");
            break;

         case "ORDERENGLISH":
        case "ORDERSENGLISH":
            sbload('MainSb', 'orderEng');

            break;
        case "FEEDBACK":
            sbload('MainSb', 'OrderFeedback');

            break;
        case "FEEDBACKENGLISH":
            sbload('MainSb', 'OrderFeedbackEng');

            break;
        case "JOB":
        case "JOBS":
            sbload('MainSb', 'JobApp');

            break;
        default:
            // controller.open('msngr');
            //   currentlyOpenSidebar = 'msngr';
            break;



    }



}

function SelectItemProp(prop_type, prop_id) {
    let selected_acc = "Acc_" + prop_type;
    let selected_field_name = "selected_" + prop_type;
    let selected_title_element_name = "title_for_" + prop_type + "_" + prop_id;
    let selected_field = ""; selected_field = document.getElementById(selected_field_name);
    let selected_data = ""; selected_data = document.getElementById(prop_type);
    let selected_title = ""; selected_title = document.getElementById(selected_title_element_name).value;
   
    selected_field.innerHTML = selected_title;
    selected_data.value = prop_id;
    Accordionize(selected_acc);
    CalcItemCost();
}
function CartChangeQuan(theAction) {
    theAction = theAction || "+1";
    let displayElement = document.getElementById("quanOfPackagesLabel");
    let InputElement = document.getElementById("quanOfPackages");
    let curQuan = parseInt(InputElement.value, 10);
    console.log(InputElement.value)
    console.log(theAction)
    console.log(curQuan)
    if (theAction == "+1") {InputElement.value = curQuan + 1;}
    if (theAction == "-1") {
        if (curQuan > 1) {InputElement.value = curQuan - 1;}
    }
    displayElement.innerHTML = InputElement.value;

}