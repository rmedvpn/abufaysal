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
              //  window.location.href = "/Login";
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
        case "SubmitFeedback":
        case "ChangePw":
        case "AddAddress":
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
                case "Logout":
                    window.location.href = "Login";


                    Notify(theRes.replace("!$!", ""), 1);
                    break;
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
                    if (!theRes.startsWith("!$!")) {

                        PageLoad("ORDERS");

                    }
                    else {
                        document.getElementById('AddToCartErr').innerHTML = theRes.replace("!$!", "");
                        showElement('AddToCartErr');

                    }

                    break;

                case "CartAction":
                    if (theRes.startsWith("!!!")) {
                        PageLoad("ORDERS");
                    }
                    else {
                        AjaxUpdate('CheckoutItems', 'CheckoutItemsDiv', 'MainLoader', 'CalcItemCostForm');
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



                case "SubmitFeedback":
                    sbload('MainMenu', 'Caller?p1=MainMenu');
                    Notify(theRes.replace("!$!", ""), 1);


                    break;




                case "ChangePw":
                    if (!theRes.startsWith("!$!")) {
                        Notify(theRes.replace("!$!", ""), 1);
                        sbload('BottomUp', 'Caller?p1=MemberInfo');
                    }
                    else {
                        document.getElementById('pwChangeErrDiv').innerHTML = theRes.replace("!$!", "");
                        FixAccordion('Acc_ChP');

                    }
                  
                case "UpdateMemberInfo":
                case "DeleteAddress":
                    if (!theRes.startsWith("!$!")) {
                        Notify(theRes.replace("!$!", ""), 1);
                        sbload('BottomUp', 'Caller?p1=MemberInfo');
                    }
                    else {
                        document.getElementById('UpdateMemberInfoErrDiv').innerHTML = theRes.replace("!$!", "");
                        FixAccordion('Acc_UpdInf');

                    }

                    break;
                    

                case "AddAddress":
                    if (!theRes.startsWith("!$!")) {
                        Notify(theRes.replace("!$!", ""), 1);
                        sbload('BottomUp', 'Caller?p1=MemberInfo');
                    }
                    else {
                        document.getElementById('AddAddressErrDiv').innerHTML = theRes.replace("!$!", "");
                        FixAccordion('AccAddAress');
                        FixAccordion('Acc_MemberAddress');
                        
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
    let cartItemsCnt = 0;
    if (document.getElementById('cartItemsCnt')) {
        cartItemsCnt = document.getElementById('cartItemsCnt').value;
        console.log('cartItemsCnt' + cartItemsCnt);
}
    if (
        document.getElementById('quan').value.trim() !== "" &&
        document.getElementById('strain').value.trim() !== "" &&
        document.getElementById('paper').value.trim() !== "" &&
        document.getElementById('mixture').value.trim() !== "" &&
        document.getElementById('MixturePotency').value.trim() !== ""
    ) {
        hideElement("AddToCartErr");
        AjaxUpdate('CalcItemCost', 'ItemCostReturnContainer', 'MainLoader', 'CalcItemCostForm', cartItemsCnt);
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


            switch (theAction) {
                case "PwChangeVerify":
                    FixAccordion('Acc_ChP');
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

function SelectItemProp(prop_type, prop_id,prop_value) {
    prop_value = prop_value || "";
    let selected_acc = "Acc_" + prop_type;
    let selected_field_name = "selected_" + prop_type;
    let selected_title_element_name = "title_for_" + prop_type + "_" + prop_id;
    let selected_field = ""; selected_field = document.getElementById(selected_field_name);
    let selected_data = ""; selected_data = document.getElementById(prop_type);
    let selected_title = ""; selected_title = document.getElementById(selected_title_element_name).value;

    if (prop_type == "paper") {
        let paper_color = document.getElementById("paper_color").value;
        selected_title += " " + paper_color;
    }
    if (prop_type == "MixturePotency") {
        console.log("prop_value " + prop_value);
        if (prop_value == 100) {
            hideElement('MixtureSelectionDiv');
            document.getElementById('mixture').value = "-1";
        }
        else {
            if (document.getElementById('mixture').value == "-1") {
                document.getElementById('mixture').value = "";
                document.getElementById('selected_mixture').innerHTML = "";
            }

           

            showElement('MixtureSelectionDiv');
        }
    }

    selected_field.innerHTML = selected_title;
    selected_data.value = prop_id;
    Accordionize(selected_acc);
    CalcItemCost();
}
function fbHover(fldName,fbId,StarRatings) {
    fldName = fldName || "";
    fbId = fbId || 0;
    StarRatings = StarRatings || "";
    let isValue = false;

    let valueFld = "fbVal_" + fbId;
    if (document.getElementById(valueFld)) {
        if (document.getElementById(valueFld).value != "") {
            isValue = true;

        }
    }
    let clsName = "FbStarOn";
    console.log("isValue " + isValue);
    if (!isValue) {
        fbHoverOff(fbId);
        for (var i = 1; i <= StarRatings; i++) {
            let curFld = "fb_" + fbId + "_" + i;
            var fld;

            if (document.getElementById(curFld)) {
                fld = document.getElementById(curFld);
                fld.className = clsName;
                console.log(curFld)
            }
            // loop body here
        }

    }
}
function fbHoverOff(fbId,isForce) {
    fbId = fbId || 0;
    isForce = isForce || false;
    let isValue = false;
    let valueFld = "fbVal_" + fbId;
    let clsName = "FbStar";
    
    var fld;

    if (!isForce) {
        if (document.getElementById(valueFld)) {
            if (document.getElementById(valueFld).value != "") {
                isValue = true;

            }
        }
    }
    

    if (!isValue) {
        for (var i = 1; i <= 10; i++) {
            let curFld = "fb_" + fbId + "_" + i;
            if (document.getElementById(curFld)) {
                fld = document.getElementById(curFld);
                fld.className = clsName;
                console.log(curFld)
            }
            // loop body here
        }
    }
    

    

    console.log(isValue)
    console.log(fbId)

}
function fbSelect(fbId, StarRatings) {
    fbId = fbId || 0;
    StarRatings = StarRatings || 0;

    let clsName = "FbStar";
    var fld;
    let curRating = document.getElementById("fbVal_" + fbId).value  ;

    if (curRating > 0) {
        for (var i = 1; i <= curRating; i++) {
            let curFld = "fb_" + fbId + "_" + i;
            document.getElementById(curFld).className = clsName
        }
    }
    clsName = "FbStarOn";
    for (var i = 1; i <= StarRatings; i++) {
        let curFld = "fb_" + fbId + "_" + i;
        document.getElementById(curFld).className = clsName
    }
    document.getElementById("fbVal_" + fbId).value = StarRatings;
}

function CartChangeQuan(theAction) {
    theAction = theAction || "+1";
    let displayElement = document.getElementById("quanOfPackagesLabel");
    let InputElement = document.getElementById("quanOfPackages");
    let UnitPriceElement = document.getElementById("unit_price");
    let UnitPricedisplayElement = document.getElementById("CartPriceLabel");
    ///
    let TotalGramElement = document.getElementById("total_gram");
    let TotalGramdisplayElement = document.getElementById("total_gram_display_element");
    let TotalPotentGramElement = document.getElementById("total_potent_gram");
    let TotalPotentGramdisplayElement = document.getElementById("total_potent_gram_display_element");
    let TotalMixtureGramElement = document.getElementById("total_mixture_gram");
    let TotalMixtureGramdisplayElement = document.getElementById("total_mixture_gram_display_element");

    let curQuan = parseInt(InputElement.value, 10);
    let unit_price = parseFloat(UnitPriceElement.value); // keep decimals
    let total_gram = parseFloat(TotalGramElement.value); // keep decimals
    let total_potent_gram = parseFloat(TotalPotentGramElement.value); // keep decimals
    let total_mixture_gram = parseFloat(TotalMixtureGramElement.value); // keep decimals

    if (theAction == "+1") {
        InputElement.value = curQuan + 1;
    }
    if (theAction == "-1") {
        if (curQuan > 1) {
            InputElement.value = curQuan - 1;
        }
    }

    curQuan = parseInt(InputElement.value, 10);
    let tot_price = curQuan * unit_price;
    let new_total_gram = curQuan * total_gram;
    let new_total_potent_gram = curQuan * total_potent_gram;
    let new_total_mixture_gram = curQuan * total_mixture_gram;

    // Format currency, show decimals only if needed
    let formattedPrice = tot_price.toLocaleString("he-IL", {
        style: "currency",
        currency: "ILS",
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    });
    console.log("unit_price " + formattedPrice)

    displayElement.innerHTML = InputElement.value;
    UnitPricedisplayElement.innerHTML = formattedPrice;

    // Format grams with max 1 decimal, only if needed
    function formatGrams(value) {
        return value.toLocaleString("he-IL", {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        });
    }

    TotalGramdisplayElement.innerHTML = formatGrams(new_total_gram);
    TotalPotentGramdisplayElement.innerHTML = formatGrams(new_total_potent_gram);
    TotalMixtureGramdisplayElement.innerHTML = formatGrams(new_total_mixture_gram);
}
