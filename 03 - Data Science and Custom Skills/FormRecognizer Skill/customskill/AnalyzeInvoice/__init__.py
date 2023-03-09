import logging
import json
import os
import logging
import datetime
from json import JSONEncoder
import azure.functions as func
from azure.ai.formrecognizer import FormRecognizerClient
from azure.ai.formrecognizer import FormTrainingClient
from azure.core.credentials import AzureKeyCredential

class DateTimeEncoder(JSONEncoder):
        #Override the default method
        def default(self, obj):
            if isinstance(obj, (datetime.date, datetime.datetime)):
                return obj.isoformat()

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Invoked AnalyzeInvoice Skill.')
    try:
        body = json.dumps(req.get_json())
        if body:
            logging.info(body)
            result = compose_response(body)
            return func.HttpResponse(result, mimetype="application/json")
        else:
            return func.HttpResponse(
                "Invalid body",
                status_code=400
            )
    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
def compose_response(json_data):
    values = json.loads(json_data)['values']
    
    # Prepare the Output before the loop
    results = {}
    results["values"] = []
    endpoint = os.environ["FORMS_RECOGNIZER_ENDPOINT"]
    key = os.environ["FORMS_RECOGNIZER_KEY"]
    form_recognizer_client = FormRecognizerClient(endpoint, AzureKeyCredential(key))
    for value in values:
        output_record = transform_value(value, form_recognizer_client)
        if output_record != None:
            results["values"].append(output_record)
    return json.dumps(results, ensure_ascii=False, cls=DateTimeEncoder)

## Perform an operation on a record
def transform_value(value, form_recognizer_client):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None
    # Validate the inputs
    try:         
        assert ('data' in value), "'data' field is required."
        data = value['data']   
        print(data)
        form_url = data["formUrl"]  + data["formSasToken"]   
        print(form_url)
        poller = form_recognizer_client.begin_recognize_invoices_from_url(form_url)
        invoices = poller.result()
        invoiceResults = []
        
        for idx, invoice in enumerate(invoices):
            invoiceResult = {}
            amount_due = invoice.fields.get("AmountDue")
            if amount_due:
                 invoiceResult["AmountDue"] = amount_due.value
            billing_address = invoice.fields.get("BillingAddress")
            if billing_address:
                 invoiceResult["BillingAddress"] = billing_address.value
            billing_address_recipient = invoice.fields.get("BillingAddressRecipient")
            if billing_address_recipient:
                 invoiceResult["BillingAddressRecipient"] = billing_address_recipient.value
            customer_address = invoice.fields.get("CustomerAddress")
            if customer_address:
                invoiceResult["CustomerAddress"] = customer_address.value
            customer_address_recipient = invoice.fields.get("CustomerAddressRecipient")
            if customer_address_recipient:
                invoiceResult["CustomerAddressRecipient"] = customer_address_recipient.value
            due_date = invoice.fields.get("DueDate")
            if due_date:
                invoiceResult["DueDate"] = due_date.value
            invoice_date = invoice.fields.get("InvoiceDate")
            if invoice_date:
                invoiceResult["InvoiceDate"] = invoice_date.value
            invoice_id = invoice.fields.get("InvoiceId")
            if invoice_id:
                invoiceResult["InvoiceId"] = invoice_id.value
            invoice_total = invoice.fields.get("InvoiceTotal")
            if invoice_total:
                invoiceResult["InvoiceTotal"] = invoice_total.value
            vendor_address = invoice.fields.get("VendorAddress")
            if vendor_address:
                invoiceResult["VendorAddress"] = vendor_address.value
            vendor_name = invoice.fields.get("VendorName")
            if vendor_name:
                invoiceResult["VendorName"] = vendor_name.value
            sub_total = invoice.fields.get("SubTotal")
            if sub_total:
                invoiceResult["SubTotal"] = sub_total.value
            total_tax = invoice.fields.get("TotalTax")
            if sub_total:
                invoiceResult["TotalTax"] = total_tax.value
    
            invoiceResults.append(invoiceResult)
            #BillingAddress,BillingAddressRecipient,AmountDue,SubTotal,TotalTax,

    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
            })
    except Exception as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + str(error) }   ]       
            })
    return ({
            "recordId": recordId,   
            "data": {
                "invoices": invoiceResults
            }
            })
