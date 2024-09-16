import os
import requests
import logging

def configure_ca_root_certs():
    """
    Configures the CA root certificates for the requests module.

    Returns:
        None

    """
    # Set the CA root certificates for the requests module
    # check if the SSL_CERT_FILE environment variable is set
    if 'SSL_CERT_FILE' in os.environ:   
        print('SSL_CERT_FILE environment variable is set. Setting the CA root certificates for the requests module.')
        logging.info('SSL_CERT_FILE environment variable is set. Setting the CA root certificates for the requests module.')    
        requests.utils.DEFAULT_CA_BUNDLE_PATH = os.environ['SSL_CERT_FILE']
        requests.adapters.DEFAULT_CA_BUNDLE_PATH = os.environ['SSL_CERT_FILE']
    #os.path.join(os.environ['AzureWebJobsScriptRoot'], 'lib', 'cacert.pem')
