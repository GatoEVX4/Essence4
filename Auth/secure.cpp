//#include "ThemidaSDK.h"

#include <jwt-cpp/jwt.h>
#include <windows.h>
#include <psapi.h>
#include <wincrypt.h>
#include <iostream>
#include <string>
#include <sstream>
#include <fstream>
#include <random>
#include <cstdio>
#include <ctime>
#include <cstring>
#include <regex> 
#include <vector>
#include <iomanip>
#include <buffer.h>

#include <openssl/evp.h>
#include <openssl/rand.h>
#include <openssl/sha.h>
#include <openssl/aes.h>


#include <thread>
#include <chrono>
#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio.hpp>
#include <boost/asio/ssl.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/strand.hpp>
#include <boost/algorithm/string.hpp>
#include <boost/beast/websocket/ssl.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <nlohmann/json.hpp>
#include <curl/curl.h>


#include <comdef.h>
#include <Wbemidl.h>
#include <intrin.h>
#include <tlhelp32.h>

#pragma comment(lib, "wbemuuid.lib")



std::string apphash = "";
std::string updhash = "";
std::string fh = "";

const std::string url = "https://essenceapi.discloud.app/";

std::string D(const std::vector<int>& x) {
    std::string result; result.reserve(x.size());
    for (int i = x.size() - 1; i >= 0; --i) {
        int v = x[i] ^ 0xA;
        result.push_back(static_cast<char>(v + (3 * (v % 2 == 0 ? 1 : -1))));
    }
    return result;
}

const std::string K1 = D({ 102,123,123,61,120,96,115,109,51,51,36,51,98,125,113,115,63,62,119,100,96,84,49,119,111,63,127,125,56,108,98,109 });
const std::string K2 = D({ 58,51,127,120,98,109,108,126,101,119,49,98,115,51,122,124,56,103,58,100,127,57,127,57,125,56,111,115,49,58,104,121 });
const std::string H_S1 = D({ 120,110,110,111,99,119,98,110,49,100,58,125,106,58,58,111,57,60,63,102,100,104,126,106,111,108,57,102,62,62,100,96 });
const std::string H_S2 = D({ 120,113,125,60,96,122,125,111,127,119,100,99,100,61,102,110,120,104,126,109,49,120,51,103,62,84,100,108,99,125,115,103 });

bool FileSHA256(const std::wstring& caminho, BYTE hash[32], std::string& hashStr) {
    bool sucesso = false;
    HCRYPTPROV hProv = 0;
    HCRYPTHASH hHash = 0;
    HANDLE hFile = CreateFileW(caminho.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);

    if (hFile == INVALID_HANDLE_VALUE) return false;
    if (CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT)) {
        if (CryptCreateHash(hProv, CALG_SHA_256, 0, 0, &hHash)) {
            BYTE buffer[4096];
            DWORD bytesRead;
            while (ReadFile(hFile, buffer, sizeof(buffer), &bytesRead, NULL) && bytesRead > 0) {
                CryptHashData(hHash, buffer, bytesRead, 0);
            }
            DWORD hashSize = 32;
            if (CryptGetHashParam(hHash, HP_HASHVAL, hash, &hashSize, 0)) {
                sucesso = true;
                char hashHex[65] = { 0 };
                for (int i = 0; i < 32; i++) sprintf_s(hashHex + (i * 2), 3, "%02X", hash[i]);
                hashStr = hashHex;
            }
            CryptDestroyHash(hHash);
        }
        CryptReleaseContext(hProv, 0);
    }
    CloseHandle(hFile);
    return sucesso;
}



bool Check();

std::string b64url_encode(const std::string& data) {
    BIO* bio, * b64;
    BUF_MEM* bufferPtr;
    b64 = BIO_new(BIO_f_base64());
    bio = BIO_new(BIO_s_mem());
    bio = BIO_push(b64, bio);
    BIO_set_flags(bio, BIO_FLAGS_BASE64_NO_NL);
    BIO_write(bio, data.data(), data.size());
    BIO_flush(bio);
    BIO_get_mem_ptr(bio, &bufferPtr);
    std::string result(bufferPtr->data, bufferPtr->length);
    BIO_free_all(bio);

    std::replace(result.begin(), result.end(), '+', '-');
    std::replace(result.begin(), result.end(), '/', '_');
    result.erase(std::remove(result.begin(), result.end(), '='), result.end());
    return result;
}

std::string b64url_decode(const std::string& data) {
    std::string decoded = data;
    std::replace(decoded.begin(), decoded.end(), '-', '+');
    std::replace(decoded.begin(), decoded.end(), '_', '/');
    while (decoded.size() % 4) decoded += "=";

    BIO* bio, * b64;
    int decodeLen = decoded.size();
    std::vector<unsigned char> buffer(decodeLen);
    b64 = BIO_new(BIO_f_base64());
    bio = BIO_new_mem_buf(decoded.data(), decoded.size());
    bio = BIO_push(b64, bio);
    BIO_set_flags(bio, BIO_FLAGS_BASE64_NO_NL);
    int length = BIO_read(bio, buffer.data(), buffer.size());
    buffer.resize(length);
    BIO_free_all(bio);
    return std::string(buffer.begin(), buffer.end());
}

std::string Enc(const std::string& plaintext, const std::string& salt_str = "", const bool c = true) {
    //VM_START
    if (c && !Check()) return "";
    unsigned char salt[32];
    if (salt_str.empty()) {
        RAND_bytes(salt, sizeof(salt));
    }
    else {
        std::string decoded_salt = b64url_decode(salt_str);
        memcpy(salt, decoded_salt.data(), sizeof(salt));
    }

    std::string salt_string = K1 + "-" + b64url_encode(std::string((char*)salt, sizeof(salt)));
    unsigned char key[SHA256_DIGEST_LENGTH];
    SHA256((unsigned char*)salt_string.c_str(), salt_string.size(), key);

    unsigned char iv[AES_BLOCK_SIZE];
    memcpy(iv, salt, AES_BLOCK_SIZE);

    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    EVP_EncryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, key, iv);

    std::vector<unsigned char> ciphertext(plaintext.size() + AES_BLOCK_SIZE);
    int len;
    EVP_EncryptUpdate(ctx, ciphertext.data(), &len, (unsigned char*)plaintext.c_str(), plaintext.size());
    int ciphertext_len = len;
    EVP_EncryptFinal_ex(ctx, ciphertext.data() + len, &len);
    ciphertext_len += len;
    EVP_CIPHER_CTX_free(ctx);

    std::string encrypted_text = b64url_encode(std::string((char*)ciphertext.data(), ciphertext_len));
    std::string salt_encoded = b64url_encode(std::string((char*)salt, sizeof(salt)));

    return encrypted_text + "." + salt_encoded;
}


std::string Dec(const std::string& token, bool c = true, bool key2 = false) {
    if (c && !Check()) return {};

    size_t dot = token.find('.');
    if (dot == std::string::npos) return "";

    auto enc_data = b64url_decode(token.substr(0, dot));
    auto salt_str = b64url_decode(token.substr(dot + 1));

    unsigned char salt[32], iv[AES_BLOCK_SIZE], key[SHA256_DIGEST_LENGTH];
    memcpy(salt, salt_str.data(), sizeof(salt));
    memcpy(iv, salt, AES_BLOCK_SIZE);

    auto salt_input = (key2 ? K2 : K1) + "-" + b64url_encode(std::string((char*)salt, sizeof(salt)));
    SHA256((unsigned char*)salt_input.c_str(), salt_input.size(), key);

    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, key, iv);

    std::vector<unsigned char> decrypted(enc_data.size());
    int len, decrypted_len = 0;
    EVP_DecryptUpdate(ctx, decrypted.data(), &len, (unsigned char*)enc_data.c_str(), enc_data.size());
    decrypted_len += len;
    EVP_DecryptFinal_ex(ctx, decrypted.data() + len, &len);
    decrypted_len += len;
    EVP_CIPHER_CTX_free(ctx);

    return std::string((char*)decrypted.data(), decrypted_len);
}




std::string hashStrr;
void FreeFire() {
    HMODULE hModule = NULL;
    GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCTSTR)FreeFire, &hModule);

    if (hModule != NULL) {
        std::cout << "WOW Free Robux?!?!? " << hashStrr << std::endl;
        FreeLibrary(hModule);
    }

    TerminateProcess(GetCurrentProcess(), 0);
}

bool Check() {
    DWORD pid = GetCurrentProcessId(); HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
    if (hProcess) {
        wchar_t caminho[MAX_PATH];

        if (GetModuleFileNameExW(hProcess, NULL, caminho, MAX_PATH)) {
            BYTE hashCalculado[32];
            std::string hashStr;
            if (!FileSHA256(caminho, hashCalculado, hashStr)) {                
                CloseHandle(hProcess);
                FreeFire();
                return false;
            }

            //std::cout << hashStr << std::endl;
            hashStrr = hashStr;
            fh = std::string(Enc(hashStr.c_str(), "", false));

            if (hashStr != apphash && hashStr != updhash) {
                std::cout << hashStr << std::endl;
                CloseHandle(hProcess);
                return false;
            }
            else {
                CloseHandle(hProcess);
                return true;
            }
        }
    }
    FreeFire();
    return false;
}

std::string Sha256Hash(const std::string& input) {
    std::vector<unsigned char> hash(EVP_MAX_MD_SIZE); unsigned int length = 0;
    EVP_MD_CTX* ctx = EVP_MD_CTX_new();
    EVP_DigestInit_ex(ctx, EVP_sha256(), nullptr);
    EVP_DigestUpdate(ctx, input.c_str(), input.size());
    EVP_DigestFinal_ex(ctx, hash.data(), &length);
    EVP_MD_CTX_free(ctx);
    hash.resize(length);

    std::ostringstream oss;
    for (unsigned char byte : hash) oss << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(byte);
    return oss.str();
}

std::string Gen() {
    static const char chars[] = "abcdefghijklmnopqrstuvwxyz0123456789";
    std::string result; std::random_device rd; std::mt19937 gen(rd()); std::uniform_int_distribution<> dis(0, sizeof(chars) - 2);
    for (int i = 0; i < 43; ++i) result += chars[dis(gen)];
    std::string key = K1 + "-" + result;
    return Sha256Hash(key) + ":" + result;
}

std::string GetChassisType() {
    HRESULT hres = CoInitializeEx(0, COINIT_MULTITHREADED);
    if (FAILED(hres)) return "Unknown";

    IWbemLocator* pLoc = nullptr;
    IWbemServices* pSvc = nullptr;
    IEnumWbemClassObject* pEnumerator = nullptr;
    std::string chassisType = "Desktop";

    if (FAILED(CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER, IID_IWbemLocator, (LPVOID*)&pLoc)) ||
        FAILED(pLoc->ConnectServer(_bstr_t(L"ROOT\\CIMV2"), nullptr, nullptr, nullptr, 0, nullptr, 0, &pSvc)) ||
        FAILED(CoSetProxyBlanket(pSvc, RPC_C_AUTHN_WINNT, RPC_C_AUTHZ_NONE, nullptr, RPC_C_AUTHN_LEVEL_CALL, RPC_C_IMP_LEVEL_IMPERSONATE, nullptr, EOAC_NONE)) ||
        FAILED(pSvc->ExecQuery(_bstr_t("WQL"), _bstr_t("SELECT * FROM Win32_SystemEnclosure"), WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, nullptr, &pEnumerator))) {
        if (pLoc) pLoc->Release();
        if (pSvc) pSvc->Release();
        if (pEnumerator) pEnumerator->Release();
        CoUninitialize();
        return "Unknown";
    }

    IWbemClassObject* pclsObj = nullptr;
    ULONG uReturn = 0;
    VARIANT vtProp = {}; // Inicializando vtProp

    while (pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn) == S_OK && uReturn > 0) {
        if (SUCCEEDED(pclsObj->Get(L"ChassisTypes", 0, &vtProp, 0, 0)) && vtProp.vt == (VT_ARRAY | VT_I2)) {
            short* pChassisTypes = nullptr;
            SafeArrayAccessData(vtProp.parray, (void**)&pChassisTypes);
            for (LONG i = 0; i < static_cast<LONG>(vtProp.parray->rgsabound[0].cElements); i++) {
                if (pChassisTypes[i] == 8 || pChassisTypes[i] == 9 || pChassisTypes[i] == 10) {
                    chassisType = "Notebook";
                    break;
                }
            }
            SafeArrayUnaccessData(vtProp.parray);
            VariantClear(&vtProp);
        }
        pclsObj->Release();
    }

    if (pLoc) pLoc->Release();
    if (pSvc) pSvc->Release();
    if (pEnumerator) pEnumerator->Release();
    CoUninitialize();

    return chassisType;
}

std::string GenHWID() {
    std::string processorId = "";
    try {
        int cpuInfo[4] = { -1 };
        char id[50] = { 0 };
        __cpuid(cpuInfo, 1);
        sprintf_s(id, sizeof(id), "%08X%08X", cpuInfo[0], cpuInfo[3]);
        processorId = std::string(id);
    }
    catch (...)
    {
        processorId = "failed";
    }

    std::string volumeSerial = "";
    try {
        char windowsPath[MAX_PATH];
        if (GetWindowsDirectoryA(windowsPath, MAX_PATH)) {
            std::string systemDrive = std::string(1, windowsPath[0]) + ":\\";

            DWORD serialNumber;
            if (GetVolumeInformationA(systemDrive.c_str(), NULL, 0, &serialNumber, NULL, NULL, NULL, 0)) {
                char serial[9];
                sprintf_s(serial, "%08X", serialNumber);
                volumeSerial = std::string(serial);
            }
        }
    }
    catch (...)
    {
        volumeSerial = "failed";
    }

    std::string hwid = processorId + volumeSerial;
    return hwid;
}


std::string devicekind = "unknown";
std::string token = "null";

using namespace std;
using namespace std::chrono;
const std::string chars = "abcdefghijklmnopqrstuvwxyz";

namespace asio = boost::asio;
namespace beast = boost::beast;         // from <boost/beast.hpp>
namespace http = beast::http;           // from <boost/beast/http.hpp>
namespace websocket = beast::websocket; // from <boost/beast/websocket.hpp>
namespace net = boost::asio;            // from <boost/asio.hpp>
namespace ssl = boost::asio::ssl;       // from <boost/asio/ssl.hpp>
using tcp = boost::asio::ip::tcp;       // from <boost/asio/ip/tcp.hpp>
using json = nlohmann::json;

asio::io_context io_context;
std::unique_ptr<websocket::stream<ssl::stream<tcp::socket>>> client = nullptr;
std::unique_ptr<std::thread> io_thread = nullptr;


std::unordered_map<std::string, std::string> sclaims;
bool isConnected = false;
bool repeat = false;
bool repeat2 = false;
bool starting_connection = false;
typedef void (*ServerCallback)(const char* message);
typedef void (*DataStreamBack)(const char* message);
const char* retryUntilSuccess(const char* finall, void (*srvcall)(const char*));



size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* output) {
    size_t totalSize = size * nmemb;
    output->append(static_cast<char*>(contents), totalSize);
    return totalSize;
}
std::string makeGetRequest(const std::string& url) {
    CURL* curl;
    CURLcode res;
    std::string response;

    curl_global_init(CURL_GLOBAL_DEFAULT); // Inicializa a biblioteca libcurl
    curl = curl_easy_init(); // Cria um handle para a requisição

    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str()); // Define a URL
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback); // Define a função de escrita
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response); // Passa o ponteiro para a string de resposta

        res = curl_easy_perform(curl); // Executa a requisição GET

        if (res != CURLE_OK) {
            //std::cout << "Erro na requisição: " << curl_easy_strerror(res) << std::endl;
        }

        curl_easy_cleanup(curl); // Limpa o handle
    }

    curl_global_cleanup(); // Finaliza a biblioteca libcurl
    return response;
}


bool TestServer(const std::string& url) {
    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cout << "Failed to initialize curl" << std::endl;
        return false;
    }

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    curl_easy_setopt(curl, CURLOPT_TIMEOUT, 5L);
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
    curl_easy_setopt(curl, CURLOPT_NOBODY, 0L);

    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, +[](void*, size_t size, size_t nmemb, void*) -> size_t {
        return size * nmemb;
    });


    CURLcode res = curl_easy_perform(curl);
    long http_code = 0;
    if (res == CURLE_OK) {
        curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);
    }

    curl_easy_cleanup(curl);

    //std::cout << http_code << std::endl;
    return (http_code >= 200 && http_code < 300);
}


const std::string GenRequestAuth(const char* data = "Essence") {
    if (data == nullptr) data = "Essence";
    if (!Check()) return "";
    std::string dataStr = data;
    std::string lol = Gen();
    std::string input = std::string(Dec(fh)) + H_S1 + ":" + lol + ":" + dataStr + ":" + H_S2 + std::string(Dec(fh));

    unsigned char hash[SHA256_DIGEST_LENGTH];
    EVP_MD_CTX* sha256_ctx = EVP_MD_CTX_new();
    if (sha256_ctx == nullptr) { return ""; }
    if (EVP_DigestInit_ex(sha256_ctx, EVP_sha256(), nullptr) != 1) { EVP_MD_CTX_free(sha256_ctx); return ""; }
    if (EVP_DigestUpdate(sha256_ctx, input.c_str(), input.length()) != 1) { EVP_MD_CTX_free(sha256_ctx); return ""; }
    if (EVP_DigestFinal_ex(sha256_ctx, hash, nullptr) != 1) { EVP_MD_CTX_free(sha256_ctx); return ""; }
    EVP_MD_CTX_free(sha256_ctx);

    std::stringstream request_hash;
    for (int i = 0; i < SHA256_DIGEST_LENGTH; ++i)
        request_hash << std::setw(2) << std::setfill('0') << std::hex << (int)hash[i];

    std::string result = lol + "|" + request_hash.str();
    return result;
}


int CheckResponse(const std::string authenticityStr, const std::string originalSeed, const std::string dataStr, const bool c = true) {
    if (c && !Check()) return 666;

    if (authenticityStr.length() < 30) return 1;

    size_t pos = authenticityStr.find("|");
    if (pos == std::string::npos) return 1;

    std::string authenticity2Part = authenticityStr.substr(0, pos);
    std::string requestHash = authenticityStr.substr(pos + 1);
    size_t colonPos = authenticity2Part.find(":");
    if (colonPos == std::string::npos) return 1;

    std::string chave = authenticity2Part.substr(0, colonPos);
    std::string n = authenticity2Part.substr(colonPos + 1);

    if (n != originalSeed) return 2;

    if (Sha256Hash(K1 + "-" + n) != chave) return 3;

    if (Sha256Hash(H_S1 + ":" + chave + ":" + n + ":" + dataStr + ":" + H_S2) != requestHash) return 4;
    return 0;
}


std::string StartWS(ServerCallback srvcall) {
    std::cout << "Connecting To Server..." << std::endl;
    if (starting_connection) {
        while (starting_connection)
            std::this_thread::sleep_for(std::chrono::milliseconds(1000));
        return "";
    }
   
    starting_connection = true;

    if (!TestServer("https://essenceapi.discloud.app")) {
        if (!TestServer("http://www.msftconnecttest.com/connecttest.txt"))
            srvcall("pc-offline");
        else
            srvcall("server-offline");

        std::this_thread::sleep_for(std::chrono::milliseconds(1500));
        starting_connection = false;        
        return StartWS(srvcall);
    }

    try {
        std::cout << "Starting connection" << std::endl;

        ssl::context ctx(ssl::context::tlsv12_client);
        ctx.set_default_verify_paths();

        ctx.set_verify_mode(ssl::verify_none);

        std::string host = "essenceapi.discloud.app";
        std::string target = "/internals/heartbeat";
        std::string port = "443";

        std::cout << "resolving..." << std::endl;
        tcp::resolver resolver(io_context);
        auto const results = resolver.resolve(host, port);
        std::cout << "tcp connect..." << std::endl;
        tcp::socket tcp_socket(io_context);
        asio::connect(tcp_socket, results.begin(), results.end());
        ssl::stream<tcp::socket> ssl_stream(std::move(tcp_socket), ctx);

        if (!SSL_set_tlsext_host_name(ssl_stream.native_handle(), host.c_str())) {
            boost::system::error_code ec{ static_cast<int>(::ERR_get_error()), asio::error::get_ssl_category() };
            throw boost::system::system_error(ec);
        }

        std::cout << "ssl handshake..." << std::endl;
        ssl_stream.handshake(ssl::stream_base::client);
        client = std::make_unique<websocket::stream<ssl::stream<tcp::socket>>>(std::move(ssl_stream));
        client->handshake(host, target);

        std::cout << "Connected." << std::endl;

        std::string lol = GenRequestAuth(token.c_str());
        json message = {
            {"etoken", token},
            {"authenticity", lol}
        };

        std::string message_str = message.dump();
        client->write(asio::buffer(message_str));
        std::cout << "Sent." << std::endl;

        try {
            std::array<char, 5000> buffer;
            boost::system::error_code error;
            size_t len = client->read_some(boost::asio::buffer(buffer), error);

            if (error == boost::asio::error::eof) {
                std::cout << "Connection closed by server." << std::endl;
                starting_connection = false;
                srvcall("server-online");
                return "verification-pending";
            }
            else if (error) {
                std::cout << "Error during receiving message: " << error.message() << std::endl;
            }
            else {
                isConnected = true;
                starting_connection = false;
                std::string response(buffer.data(), len);
                json parsed = json::parse(response);
                std::string authenticity = parsed["authenticity"];
                size_t pos = response.find("\"user\":");
                if (pos != std::string::npos) {
                    size_t end = response.find(",\"authenticity\":", pos);
                    std::string user = response.substr(pos + 7, end - (pos + 7));

                    std::cout << "User: " << user << std::endl;
                    std::cout << "Authenticity: " << authenticity << "...." << std::endl;

                    int xd = CheckResponse(authenticity, lol.substr(0, lol.find("|")).substr(lol.find(":") + 1), user);
                    std::cout << xd << std::endl;

                    if (xd == 0) {
                        srvcall("server-online");
                        return user;
                    }
                }
            }
        }
        catch (const std::exception& e) {
            std::cout << "Exception: " << e.what() << std::endl;
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(2000));

        starting_connection = false;
        srvcall("server-offline");
        return StartWS(srvcall);

            //if (!io_thread) {
            //    io_thread = std::make_unique<std::thread>([] {
            //        io_context.run();
            //    });
            //}
    }
    catch (const boost::system::system_error& e) {
        starting_connection = false;
        isConnected = false;
        std::cout << std::endl << std::endl << std::endl << "ERROR" << std::endl;
        std::cout << "Error code: " << e.code() << std::endl;
        std::cout << "Error message: " << e.what() << std::endl;
        srvcall("server-offline");
        std::this_thread::sleep_for(std::chrono::milliseconds(3500));
        return StartWS(srvcall);
    }
}


const char* DecJwt(const char* token) {    
    try {        
        auto decoded = jwt::decode(token);
        jwt::verify().allow_algorithm(jwt::algorithm::hs256{ K2 }).verify(decoded);

        auto payload = decoded.get_payload_json();

        std::ostringstream oss;
        oss << "{";
        bool first = true;
        for (const auto& claim : payload) {
            if (!first) oss << ",";
            oss << "\"" << claim.first << "\":";
            oss << "\"" << claim.second.get<std::string>() << "\"";

            first = false;
        }
        oss << "}";    
        return _strdup(oss.str().c_str());
    }
    catch (const std::exception& ex) {
        //std::cout << "Erro ao decodificar JWT: " << ex.what() << std::endl;
        return nullptr;
    }
}


extern "C" __declspec(dllexport) void Execute(const std::string& src) {
    WSADATA wsaData;
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0) {
        std::cerr << "WSAStartup failed with error: " << result << std::endl;
        return;
    }

    SOCKET sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (sock == INVALID_SOCKET) {
        std::cerr << "Socket creation failed with error: " << WSAGetLastError() << std::endl;
        WSACleanup();
        return;
    }

    sockaddr_in serverAddr = { 0 };
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(4555);
    InetPtonA(AF_INET, "127.0.0.1", &serverAddr.sin_addr);

    result = connect(sock, (struct sockaddr*)&serverAddr, sizeof(serverAddr));
    if (result == SOCKET_ERROR) {
        std::cerr << "Connection failed with error: " << WSAGetLastError() << std::endl;
        closesocket(sock);
        WSACleanup();
        return;
    }

    std::string e = Sha256Hash(K1 + "-" + src) + ":" + src;
    send(sock, e.c_str(), e.size(), 0);
    closesocket(sock);
    WSACleanup();
}



extern "C" __declspec(dllexport) const char* GenDownloadRA() {
    //VM_START
    if (!Check()) return nullptr;
    //VM_END

    return strdup(GenRequestAuth("DWNF").c_str());
}

extern "C" __declspec(dllexport) const char* GetHWID()
{
    if (!Check()) return nullptr;
    return strdup(GenHWID().c_str());
}

extern "C" __declspec(dllexport) void DiscordAuth(const int port){
    if (!Check()) return;

    std::string auth_link = "https://discord.com/api/oauth2/authorize?client_id=1336373573744332963"
        "&redirect_uri=https://essenceapi.discloud.app/externals/oauth_redirect"
        "&response_type=code&scope=email+identify+guilds.join"
        "&state=1210371208252760074..." + std::to_string(port) + "..." + GenRequestAuth(token.c_str()) + "..." + std::string(token);

    std::cout << "Discord AUTH link: " << auth_link << std::endl;

    std::wstring wide_auth_link(auth_link.begin(), auth_link.end());
    ShellExecuteW(NULL, L"open", wide_auth_link.c_str(), NULL, NULL, SW_SHOWNORMAL);
}

extern "C" __declspec(dllexport) const char* Decrypt(const char* encryptedtoken) {
    if (!Check()) return nullptr;
    return strdup(Dec(encryptedtoken, true, true).c_str());
}

extern "C" __declspec(dllexport) long long TimeLeft(const std::string& file) {
    if (!Check()) return 1;

    try {
        DWORD fileAttributes = GetFileAttributesA(file.c_str());
        if (fileAttributes == INVALID_FILE_ATTRIBUTES || (fileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
            return 1;
        }

        std::ifstream inFile(file);
        if (!inFile.is_open()) {
            return 1;
        }

        std::string dados((std::istreambuf_iterator<char>(inFile)), std::istreambuf_iterator<char>());
        inFile.close();

        std::vector<std::string> parts;
        std::stringstream ss(dados);
        std::string part;
        while (std::getline(ss, part, '.')) {
            parts.push_back(part);
        }

        if (parts.size() != 2) {
            return 1;
        }

        std::string result = Dec(dados.c_str());
        std::tm tm = {};
        std::istringstream iss(result);
        iss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

        if (iss.fail()) {
            return 1;
        }

        std::time_t savedTime = std::mktime(&tm);
        std::time_t now = std::time(nullptr);
        double difference = std::difftime(savedTime, now);
        if (difference > 0 && difference < 12 * 60 * 60) {
            return static_cast<long long>(difference * 1000);
        }
        return 1;
    }
    catch (...) {
        return 1;
    }
}

extern "C" __declspec(dllexport) const char* DoJWT(const char* claims) {
    if (!Check()) return nullptr;
    try {
        if (claims != nullptr && std::strlen(claims) > 0) {
            std::string claims_str(claims);
            claims_str += ",";

            size_t pos = 0;
            while ((pos = claims_str.find(",")) != std::string::npos) {
                std::string pair = claims_str.substr(0, pos);
                size_t colon_pos = pair.find(":");
                if (colon_pos != std::string::npos) {
                    std::string key = pair.substr(0, colon_pos);
                    std::string value = pair.substr(colon_pos + 1);
                    sclaims[key] = value;
                }
                claims_str.erase(0, pos + 1);
            }
        }

        auto token_builder = jwt::create().set_type("JWT");

        for (const auto& claim : sclaims) {
            token_builder.set_payload_claim(claim.first, jwt::claim(claim.second));
        }
        token_builder.set_payload_claim("hwid", jwt::claim(GenHWID()));
        auto token2 = token_builder.sign(jwt::algorithm::hs256{ K2 });
        char* result = new char[token2.size() + 1];
        std::strcpy(result, token2.c_str());

        token = result;
        return result;
    }
    catch (...) {
        return nullptr;
    }
}

int hidh2 = 0;
extern "C" __declspec(dllexport) const char* RequestResource(const char* res, const char* extradata, ServerCallback srvcall, DataStreamBack datastream = nullptr, bool force_return = false) {
    if (!Check()) return nullptr;

    std::cout << "\n\n\nREQUEST RESOURCE: " << res << std::endl;
    std::cout << "EXTRADATA: " << extradata << std::endl;

    if (!isConnected || !client || !client->is_open()) {
        std::cout << "isConnected: " << (isConnected ? "true" : "false") << std::endl;
        srvcall("server-offline");
        StartWS(srvcall);
        RequestResource(res, extradata, srvcall, datastream, force_return);
        return "error";
    }

    std::string original_authenticity = GenRequestAuth(extradata);
    std::string n = original_authenticity.substr(0, original_authenticity.find("|")).substr(original_authenticity.find(":") + 1);

    json message = {
        {"resource", res},
        {"authenticity", original_authenticity},
        {"data", extradata}
    };

    std::string message_str = message.dump();
    client->write(asio::buffer(message_str));

    beast::flat_buffer buffer;
    std::string responseMessage;
    while (client->is_open()) {
        try {
            client->read(buffer);
            responseMessage = beast::buffers_to_string(buffer.data());
            buffer.consume(buffer.size());

            if (datastream != nullptr) {
                datastream(responseMessage.c_str());
                continue;
            }

            json parsed = json::parse(responseMessage);
            std::string authenticity = parsed["authenticity"];
            size_t pos = responseMessage.find("\"response\":");
            if (pos != std::string::npos) {
                size_t end = responseMessage.find(",\"authenticity\":", pos);
                std::string response = responseMessage.substr(pos + 11, end - (pos + 11));

                std::cout << "User: " << response << std::endl;
                std::cout << "Authenticity: " << authenticity << "...." << std::endl;

                int xd = CheckResponse(authenticity, n, response);
                std::cout << xd << std::endl;

                if (xd == 0) {
                    srvcall("server-online");
                    return strdup(response.c_str());
                }
            }
            

            if (force_return) {
                return "error";
            }
            else {
                if (repeat2) {
                    repeat2 = false;
                    srvcall("server-offline");
                }
                else {
                    repeat2 = true;
                }
                if (hidh2 < 4) hidh2++;
                std::this_thread::sleep_for(std::chrono::seconds(2 * hidh2));
                return RequestResource(res, extradata, srvcall, datastream, force_return);
            }
        }
        catch (std::exception const& e) {
            std::string errorMsg = "[ERROR] Falha ao enviar dados: " + std::string(e.what());

            if (force_return) {
                return "error";
            }

            if (repeat2) {
                repeat2 = false;
                srvcall("server-offline");
            }
            else {
                repeat2 = true;
            }

            if (hidh2 < 4) hidh2++;
            std::this_thread::sleep_for(std::chrono::seconds(2 * hidh2));
            StartWS(srvcall);
            return RequestResource(res, extradata, srvcall, datastream, force_return);
        }
    }
    return "end";
}

extern "C" __declspec(dllexport) const char* PostReq(const char* finalk, bool force_return, ServerCallback srvcall) {
    if (!Check()) return nullptr;

    try {
        std::string original_authenticity = GenRequestAuth(token.c_str());
        CURL* curl = curl_easy_init();
        if (curl) {
            std::string response;
            long http_code = 0;

            // URL e dados do POST
            std::string full_url = url + finalk;
            curl_easy_setopt(curl, CURLOPT_URL, full_url.c_str());
            //curl_easy_setopt(curl, CURLOPT_VERBOSE, 1L);

            curl_easy_setopt(curl, CURLOPT_POST, 1L);
            curl_easy_setopt(curl, CURLOPT_POSTFIELDS, "");
            curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, 0L);


            std::cout << "POST REQ: " << (url + finalk).c_str() << std::endl;

            // Headers
            struct curl_slist* headers = nullptr;
            std::string n = original_authenticity.substr(0, original_authenticity.find("|")).substr(original_authenticity.find(":") + 1);
            headers = curl_slist_append(headers, ("authenticity: " + original_authenticity).c_str());
            headers = curl_slist_append(headers, ("token: " + token).c_str());
            curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);

            // Callback para escrita da resposta
            curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
            curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);

            // Executa a requisição
            CURLcode res = curl_easy_perform(curl);
            if (res == CURLE_OK) {
                curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);

                curl_easy_cleanup(curl);
                curl_global_cleanup();

                if (http_code == 429 && force_return) {
                    return "429";
                }

                if (http_code >= 200 && http_code < 300) {
                    if (response.length() < 40) {
                        if (force_return) {
                            return "777";
                        }

                        if (repeat) {
                            repeat = false;
                            srvcall("server-offline");
                        }
                        else {
                            repeat = true;
                        }
                        return retryUntilSuccess(finalk, srvcall);
                    }

                    if (!response.empty() && response.front() == '"') {
                        response = response.substr(1, response.length() - 2);
                    }

                    std::cout << response << std::endl;
                    const std::string re = Dec(response);
                    std::cout << "Response: " << re << std::endl;

                    if (re == "Errorr24542") {
                        if (force_return) {
                            return "erro";
                        }
                        srvcall("Client tried to change server response");
                        return retryUntilSuccess(finalk, srvcall);
                    }

                    return strdup(re.c_str());
                }
                else {
                    if (force_return) {
                        return "777";
                    }

                    if (repeat) {
                        repeat = false;
                        srvcall("server-offline");
                    }
                    else {
                        repeat = true;
                    }
                    return retryUntilSuccess(finalk, srvcall);
                }
            }
            curl_slist_free_all(headers);
            curl_easy_cleanup(curl);
        }
        else {
            std::cout << "Failed to initialize CURL." << std::endl;
        }

        if (force_return) {
            return "777";
        }

        if (repeat) {
            repeat = false;
            srvcall("server-offline");
        }
        else {
            repeat = true;
        }
        return retryUntilSuccess(finalk, srvcall);
    }
    catch (const std::exception& ex) {
        std::cout << "Exception: " << ex.what() << std::endl;

        if (!TestServer("http://www.msftconnecttest.com/connecttest.txt")) {
            if (force_return) {
                return "erro";
            }

            srvcall("pc-offline");
            return retryUntilSuccess(finalk, srvcall);
        }
        else {
            if (force_return) {
                return "erro";
            }

            srvcall("server-offline");
            return retryUntilSuccess(finalk, srvcall);
        }
    }
}
const char* retryUntilSuccess(const char* finall, ServerCallback srvcall) {
    int hidh = 1;
    while (true) {
        std::this_thread::sleep_for(std::chrono::seconds(2 * hidh));
        const char* lol = PostReq(finall, true, srvcall);
        if (lol != "777" && lol != "erro" && lol != "429") {
            srvcall("server-online");
            return lol;
        }

        if (hidh < 4) hidh++;
    }
};



bool inita = false;
extern "C" __declspec(dllexport) int InitAuth(const char* t) {
    std::cout << "init check" << std::endl;
    token = t;

    if (inita) {
        return 0;
    }

    CURL* curl = curl_easy_init();
    if (curl) {
        std::string response_data;
        curl_easy_setopt(curl, CURLOPT_URL, "https://essenceapi.discloud.app/externals/initcheck");

        DWORD pid = GetCurrentProcessId(); HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
        if (hProcess) {
            wchar_t caminho[MAX_PATH];

            if (GetModuleFileNameExW(hProcess, NULL, caminho, MAX_PATH)) {
                BYTE hashCalculado[32];
                std::string hashStr;
                if (!FileSHA256(caminho, hashCalculado, hashStr)) {
                    CloseHandle(hProcess);
                    FreeFire();
                    return false;
                }
                hashStrr = hashStr;
                //std::cout << hashStrr << std::endl;
                fh = std::string(Enc(hashStr.c_str(), "", false));

            }
        }

        std::string lol = Gen();
        std::string input = std::string(Dec(fh.c_str(), false)) + H_S1 + ":" + lol + ":" + "Essence" + ":" + H_S2 + std::string(Dec(fh.c_str(), false));
        std::cout << input << std::endl;

        unsigned char hash[SHA256_DIGEST_LENGTH];
        EVP_MD_CTX* sha256_ctx = EVP_MD_CTX_new();
        if (sha256_ctx == nullptr) return 1;
        if (EVP_DigestInit_ex(sha256_ctx, EVP_sha256(), nullptr) != 1) { EVP_MD_CTX_free(sha256_ctx); return 1; }
        if (EVP_DigestUpdate(sha256_ctx, input.c_str(), input.length()) != 1) { EVP_MD_CTX_free(sha256_ctx); return 1; }
        if (EVP_DigestFinal_ex(sha256_ctx, hash, nullptr) != 1) { EVP_MD_CTX_free(sha256_ctx); return 1; }
        EVP_MD_CTX_free(sha256_ctx);
        std::stringstream request_hash;
        for (int i = 0; i < SHA256_DIGEST_LENGTH; ++i)
            request_hash << std::setw(2) << std::setfill('0') << std::hex << (int)hash[i];

        std::string result = lol + "|" + request_hash.str();
        struct curl_slist* headers = nullptr;
        std::string n = result.substr(0, result.find("|")).substr(result.find(":") + 1);

        headers = curl_slist_append(headers, ("authenticity: " + result).c_str());
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);

        // Callback para escrita da resposta
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response_data);



        try {
            CURLcode res = curl_easy_perform(curl);
            if (res == CURLE_OK) {

                if (!response_data.empty() && response_data.front() == '"') {
                    response_data = response_data.substr(1, response_data.length() - 2);
                }

                if (response_data.find(".") == std::string::npos) {
                    std::cout << response_data << std::endl;
                    std::cout << "Request rejected. Prob oudated version y" << std::endl;
                    return 2;
                }

                //std::cout << response_data << std::endl;

                std::vector<std::string> parts;
                std::stringstream ss(response_data);
                std::string item;

                while (std::getline(ss, item, '.')) {
                    parts.push_back(item);
                }

                std::string part1 = parts[0];
                std::string part2 = parts[1];
                std::string part3 = parts[2];

                //std::cout << "init check request" << std::endl;
                int xd = CheckResponse(part3.c_str(), n, part1 + ":" + part2, false);

                if (xd == 0) {
                    apphash = part1;
                    //std::cout << apphash << std::endl;

                    updhash = part2;
                    //std::cout << updhash << std::endl;

                    if (Check()) {
                        std::cout << "--------------------" << std::endl << "AUTHENTICATED AND UPDATED" << std::endl << "--------------------" << std::endl;
                        inita = true;
                        return 0;
                    }
                }
            }
            else {
                std::cout << "Request failed: " << curl_easy_strerror(res) << std::endl;
                return 1;
            }

            curl_slist_free_all(headers);
            curl_easy_cleanup(curl);
        }
        catch(...){
            std::cout << "Request rejected. Prob oudated version x" << std::endl;
            return 2;
        }
    }
    else {
        std::cout << "Failed to initialize CURL." << std::endl;
        return 1;
    }

    std::cout << "init check end" << std::endl;
    return 1;
}

extern "C" __declspec(dllexport) const char* InitServer(const char* t, ServerCallback srvcall) {
    std::cout << "Init Server" << std::endl;
    if (!Check()) {
        srvcall("binary-changed");
        return "error";
    }

    token = t;
    devicekind = GetChassisType();
    std::cout << devicekind << std::endl;

    static std::string response = StartWS(srvcall); // Armazena o valor em um static
    return response.c_str();
}

extern "C" __declspec(dllexport) void CloseServer() {
    try {
        if (client && client->is_open()) {
            client->close(websocket::close_code::normal);
        }
    }
    catch(...){

    }
}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_ATTACH: 
            std::cout << "Auth Initialize" << std::endl;
            break;

        case DLL_THREAD_ATTACH:
            break;

        case DLL_THREAD_DETACH:
            break;

        case DLL_PROCESS_DETACH:
            break;
    }
    return TRUE;
}

