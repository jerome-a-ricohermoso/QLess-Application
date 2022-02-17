// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {

    $("#ddlCardType").on("change", function () {
        if ($(this).val() == 1) {
            $(".id-group").show();
        }
        else {
            $(".id-group").hide();
        }
    });

    $(".form-control").on("change", function () {
        $(this).removeClass("border border-danger");
    });

    $("#btnRegister").on("click", function () {
        var isValid = ValidateFields(".registration-form");
        if (isValid) {
            var cardType = $("#ddlCardType").val();
            var idType = $("#ddlIDType").val();
            var idNum = $("#txtID").val();
            if (idType == 1) {
                if (idNum.length != 12) {
                    $("#txtID").addClass("border border-danger");
                    isValid = false;
                }
            }
            else if (idType == 2) {
                if (idNum.length != 10) {
                    $("#txtID").addClass("border border-danger");
                    isValid = false;
                }
            }
            else {
                $("#txtID").removeClass("border border-danger");
                isValid = true;
            }

            if (isValid) {
                $.ajax({
                    url: "/Home/RegisterCard/",
                    type: "POST",
                    data: { "cardType": cardType, "idType": idType, "idNum": idNum },
                    success: function (result) {
                        if (result == 1) {
                            alert("Saved successfully!");
                            $("#ddlCardType").val(0).trigger("change");
                        }
                        else {
                            alert("Oops something went wrong!");
                        }
                    }
                });
            }
        }
    });

    $("#btnUse").on("click", function () {
        var isValid = ValidateFields(".use-form");
        var cardId = $("#hdnCardId").val();
        if (isValid) {
            var ddlFrom = $("#ddlStationFrom").val();
            var ddlTo = $("#ddlStationTo").val();
            var curBal = $("#curBal").text().replace(/^\D+/g, '');
            var minAmount = $("#hdnCardType").val() == "Normal" ? 15 : 10;
            if (ddlFrom == ddlTo) {
                alert("From and To stations can't be the same!");
                $("#ddlStationFrom, #ddlStationTo").addClass("border border-danger");
            }
            else if (curBal < minAmount) {
                alert("Insufficient funds. Please reload.");
            }
            else {
                $("#ddlStationFrom, #ddlStationTo").removeClass("border border-danger");
                $.ajax({
                    url: "/Home/UseCard/",
                    type: "POST",
                    data: { "cardId": cardId },
                    success: function (result) {
                        if (result == 1) {
                            alert("Saved successfully!");
                            $("#cardUseModal").modal("hide");
                        }
                        else {
                            alert("Oops something went wrong!");
                        }
                    }
                });
            }
        }
    });

    $("#btnReload").on("click", function () {
        var isValid = ValidateFields(".reload-form");
        if (isValid) {
            var cardId = $("#hdnCardIdReload").val();
            var remBal = parseInt($("#remBal").text().replace(/^\D+/g, ''));
            var txtCash = parseInt($("#txtCash").val());
            var txtToLoad = parseInt($("#txtToLoad").val());
            console.log(txtCash < txtToLoad);
            if (txtCash < txtToLoad) {
                $("#txtToLoad").addClass("border border-danger");
                alert("Amount to load can't be less than the cash amount.");
                isValid = false;
            }
            if (remBal >= 10000) {
                alert("Your card already reached the maximum amount of credits.");
                isValid = false;
            }
            else {
                $("#txtToLoad, #txtCash").removeClass("border border-danger");
                isValid = true;
            }

            if (isValid) {
                var amount = txtToLoad;
                var projectedAmount = txtToLoad + remBal;
                var change = txtCash - txtToLoad;
                if (projectedAmount > 10000) {
                    change = 10000 - remBal;
                    projectedAmount = 10000;
                    amount = change;
                }
                console.log("Projected amount: "+projectedAmount);
                console.log("Change: "+change);
                $.ajax({
                    url: "/Home/ReloadCard/",
                    type: "POST",
                    data: { "cardId": cardId, "amount": amount },
                    success: function (result) {
                        if (result == 1) {
                            alert("Saved successfully! Change is: " + change);
                            $("#remBal").text("Balance: ₱ " + projectedAmount);
                            $("#txtCash, #txtToLoad").val(0);
                        }
                        else {
                            alert("Oops something went wrong!");
                        }
                    }
                });
            }
        }
    });

    function ValidateFields(area) {
        var isValid = true;
        $(`${area} .form-control:visible`).each(function () {
            if ($(this).val() == 0 || $(this).val() == "" || $(this).val() == undefined) {
                isValid = false;
                $(this).addClass("border border-danger");
            }
            else {
                $(this).removeClass("border border-danger");
            }
        });
        return isValid;
    };

    $("#tblCards").DataTable();

    $(document).on("click", ".history", function () {
        var transportCardId = $(this).attr("data-transportCardId");
        $.ajax({
            url: "/Home/GetTransactions/",
            type: "POST",
            data: { "transportCardId": transportCardId },
            success: function (result) {
                $("#transactionModal").modal("show");
                $("#tblTransactions tbody").html("");
                $(result).each(function (i) {
                    $("#tblTransactions tbody").append(`
                        <tr>
                          <td>${this.transportCardId}</td>
                          <td>${this.balance}</td>
                          <td>${moment(this.createdOn).format('MM/DD/YYYY hh:mm:ss A')}</td>
                        </tr>
                    `);
                })
            }
        });
    });

    $(document).on("click", ".discounted", function () {
        var transportCardId = $(this).closest("tr").find(".transportCardId").text();
        $.ajax({
            url: "/Home/GetDetails/",
            type: "POST",
            data: { "transportCardId": transportCardId },
            success: function (result) {
                $("#cardDetailsModal").modal("show");
                $("#tblCardDetails tbody").html("");
                $(result).each(function () {
                    var idNumber = this.seniorCitizenNumber === undefined || this.seniorCitizenNumber.trim() === '' ? this.pwdidNumber : this.seniorCitizenNumber;
                    var idType = this.seniorCitizenNumber === undefined || this.seniorCitizenNumber.trim() === '' ? "Person with disability id number" : "Senior citizen control number"
                    $("#tblCardDetails tbody").append(`
                     <td>${this.transportCardId}</td>
                     <td>₱ ${this.balance}</td>
                     <td title='${idType}'>${idNumber}</td>
                    `);
                })
            }
        });
    });

    $(document).on("click", ".simulate_use", function () {
        var balance = "Balance: " + $(this).closest("tr").find(".remainingBalance").text();
        var cardType = $(this).closest("tr").find(".transportCardType").text();
        $("#curBal").text(balance);
        var transportCardId = $(this).closest("tr").find(".transportCardId").text();
        $("#hdnCardId").val(transportCardId);
        $("#hdnCardType").val(cardType);
        $("#cardUseModal").modal("show");
    });

    $(document).on("click", ".reload", function () {
        var balance = "Balance: " + $(this).closest("tr").find(".remainingBalance").text();
        var transportCardId = $(this).closest("tr").find(".transportCardId").text();
        $("#hdnCardIdReload").val(transportCardId);
        $("#remBal").text(balance);
        $("#cardReloadModal").modal("show");
    });

    $("#ddlStationTo, #ddlStationFrom").on("change", function () {
        $("#ddlStationTo, #ddlStationFrom").removeClass("border border-danger");
    })

    $("#cardUseModal, #cardReloadModal").on("hidden.bs.modal", function () {
        location.reload();
    });

})