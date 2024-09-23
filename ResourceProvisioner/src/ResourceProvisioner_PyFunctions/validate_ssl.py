import requests
import certifi
import lib.ssl_utils as ssl_utils

def check_url_status(url):
    try:
        response = requests.get(url)
        if response.status_code == 200:
            print(f"Success: Received status code {response.status_code}")
    except requests.exceptions.RequestException as e:
        print(f"An error occurred: {e}")

if __name__ == "__main__":
    url = "https://login.microsoftonline.com/"
    ssl_utils.configure_ca_root_certs()    
    check_url_status(url)
    #https://stackoverflow.com/questions/42982143/python-requests-how-to-use-system-ca-certificates-debian-ubuntu
    #https://requests.readthedocs.io/en/latest/user/advanced/#ssl-cert-verification
    ssl_path = certifi.where()
    print(f"Using CA root certificates from '{ssl_path}'")
