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
#include <evp.h>
#include <rand.h>
#include <buffer.h>


#include <thread>
#include <chrono>
#include <boost/beast.hpp>
#include <boost/asio.hpp>
#include <nlohmann/json.hpp>
#include <curl/curl.h>


#include <comdef.h>
#include <Wbemidl.h>
#include <intrin.h>
#include <tlhelp32.h>

#pragma comment(lib, "wbemuuid.lib")


const std::wstring execn = L"EssenceExperiments.exe";
const std::string exphash = "9C29E386F49B41E3C34B156F039C96FB";
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

bool HexStrToB(const std::string& hex, BYTE* bytes, size_t tamanho) {
    if (hex.length() != tamanho * 2) return false;
    for (size_t i = 0; i < tamanho; i++) sscanf_s(hex.substr(i * 2, 2).c_str(), "%02X", &bytes[i]);
    return true;
}

std::string BytesToHex(const std::vector<unsigned char>& bytes) {
    std::ostringstream oss;
    for (unsigned char byte : bytes) oss << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(byte);
    return oss.str();
}

bool FileMD5(const std::wstring& caminho, BYTE hash[16], std::string& hashStr) {
    bool sucesso = false; HCRYPTPROV hProv = 0; HCRYPTHASH hHash = 0; HANDLE hFile = CreateFileW(caminho.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);
    if (hFile == INVALID_HANDLE_VALUE) return false;
    if (CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT)) {
        if (CryptCreateHash(hProv, CALG_MD5, 0, 0, &hHash)) {
            BYTE buffer[4096]; DWORD bytesRead;
            while (ReadFile(hFile, buffer, sizeof(buffer), &bytesRead, NULL) && bytesRead > 0) CryptHashData(hHash, buffer, bytesRead, 0);
            DWORD hashSize = 16;
            if (CryptGetHashParam(hHash, HP_HASHVAL, hash, &hashSize, 0)) {
                sucesso = true; char hashHex[33] = { 0 };
                for (int i = 0; i < 16; i++) sprintf_s(hashHex + (i * 2), 3, "%02X", hash[i]);
                hashStr = hashHex;
            }
            CryptDestroyHash(hHash);
        }
        CryptReleaseContext(hProv, 0);
    }
    CloseHandle(hFile);
    return sucesso;
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
    return true;

    DWORD pid = GetCurrentProcessId(); HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
    if (hProcess) {
        wchar_t caminho[MAX_PATH];  // Use wchar_t ao invés de TCHAR

        if (GetModuleFileNameExW(hProcess, NULL, caminho, MAX_PATH)) {
            std::wstring nomeProcesso(caminho);  // Agora você pode usar diretamente o wchar_t
            size_t pos = nomeProcesso.find_last_of(L"\\/");

            if (pos != std::wstring::npos) nomeProcesso = nomeProcesso.substr(pos + 1);
            if (nomeProcesso != execn) {
                CloseHandle(hProcess);
                FreeFire();
                return false;
            }
            BYTE hashCalculado[16];
            std::string hashStr;
            if (!FileMD5(caminho, hashCalculado, hashStr)) {
                CloseHandle(hProcess);
                FreeFire();
                return false;
            }

            BYTE hashEsperadoBytes[16];
            if (!HexStrToB(exphash, hashEsperadoBytes, 16)) {
                CloseHandle(hProcess);
                FreeFire();
                return false;
            }

            if (hashStr != exphash) {
                hashStrr = hashStr;
                CloseHandle(hProcess);
                FreeFire();
                return false;
            }
            else {
                CloseHandle(hProcess);
                return true;
            }
        }

        CloseHandle(hProcess);
        FreeFire();
        return false;
    }
    CloseHandle(hProcess);
    FreeFire();
    return false;
}

std::vector<unsigned char> Sha256Hash(const std::string& input) {
    std::vector<unsigned char> hash(EVP_MAX_MD_SIZE); unsigned int length = 0;
    EVP_MD_CTX* ctx = EVP_MD_CTX_new();
    EVP_DigestInit_ex(ctx, EVP_sha256(), nullptr);
    EVP_DigestUpdate(ctx, input.c_str(), input.size());
    EVP_DigestFinal_ex(ctx, hash.data(), &length);
    EVP_MD_CTX_free(ctx);
    hash.resize(length);
    return hash;
}

std::string b64url_encode(const std::string& data) {
    BIO* bio = BIO_push(BIO_new(BIO_f_base64()), BIO_new(BIO_s_mem()));
    BIO_set_flags(bio, BIO_FLAGS_BASE64_NO_NL);
    BIO_write(bio, data.c_str(), data.length());
    BIO_flush(bio);
    BUF_MEM* bufferPtr;
    BIO_get_mem_ptr(bio, &bufferPtr);
    std::string result(bufferPtr->data, bufferPtr->length);
    BIO_free_all(bio);
    for (char& c : result) c = c == '+' ? '-' : c == '/' ? '_' : c;
    result.erase(std::remove(result.begin(), result.end(), '='), result.end());
    return result;
}

std::string b64url_decode(const std::string& data) {
    std::string modified_data = data;
    for (char& c : modified_data) c = c == '-' ? '+' : c == '_' ? '/' : c;
    while (modified_data.length() % 4 != 0) modified_data += '=';
    BIO* bio = BIO_push(BIO_new(BIO_f_base64()), BIO_new_mem_buf(modified_data.c_str(), modified_data.length()));
    BIO_set_flags(bio, BIO_FLAGS_BASE64_NO_NL);
    std::vector<char> buffer(modified_data.length());
    int length = BIO_read(bio, buffer.data(), modified_data.length());
    BIO_free_all(bio);
    return std::string(buffer.data(), length);
}

std::string add_padding(const std::string& data) {
    int padding_length = 16 - (data.length() % 16);
    return data + std::string(padding_length, (char)padding_length);
}

std::string remove_padding(const std::string& data) {
    return data.empty() || data.back() > data.size() ? data : data.substr(0, data.size() - data.back());
}

std::string Gen() {
    static const char chars[] = "abcdefghijklmnopqrstuvwxyz0123456789";
    std::string result; std::random_device rd; std::mt19937 gen(rd()); std::uniform_int_distribution<> dis(0, sizeof(chars) - 2);
    for (int i = 0; i < 30; ++i) result += chars[dis(gen)];
    std::string key = K1 + "-" +result;
    std::vector<unsigned char> hashedBytes = Sha256Hash(key);
    std::string hashedHex = BytesToHex(hashedBytes);
    return hashedHex + ":" + result;
}

std::string GetChassisType() {
    HRESULT hres = CoInitializeEx(0, COINIT_MULTITHREADED);
    if (FAILED(hres)) return "Unknown";

    hres = CoInitializeSecurity(nullptr, -1, nullptr, nullptr, RPC_C_AUTHN_LEVEL_DEFAULT, RPC_C_IMP_LEVEL_IMPERSONATE, nullptr, EOAC_NONE, nullptr);
    if (FAILED(hres)) { CoUninitialize(); return "Unknown"; }

    IWbemLocator* pLoc = nullptr;
    hres = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER, IID_IWbemLocator, (LPVOID*)&pLoc);
    if (FAILED(hres)) { CoUninitialize(); return "Unknown"; }

    IWbemServices* pSvc = nullptr;
    hres = pLoc->ConnectServer(_bstr_t(L"ROOT\\CIMV2"), nullptr, nullptr, nullptr, 0, nullptr, 0, &pSvc);
    if (FAILED(hres)) { pLoc->Release(); CoUninitialize(); return "Unknown"; }

    hres = CoSetProxyBlanket(pSvc, RPC_C_AUTHN_WINNT, RPC_C_AUTHZ_NONE, nullptr, RPC_C_AUTHN_LEVEL_CALL, RPC_C_IMP_LEVEL_IMPERSONATE, nullptr, EOAC_NONE);
    if (FAILED(hres)) { pSvc->Release(); pLoc->Release(); CoUninitialize(); return "Unknown"; }

    IEnumWbemClassObject* pEnumerator = nullptr;
    hres = pSvc->ExecQuery(_bstr_t("WQL"), _bstr_t("SELECT * FROM Win32_SystemEnclosure"), WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, nullptr, &pEnumerator);
    if (FAILED(hres)) { pSvc->Release(); pLoc->Release(); CoUninitialize(); return "Unknown"; }

    std::string chassisType = "Desktop";
    IWbemClassObject* pclsObj = nullptr;
    ULONG uReturn = 0;

    while (pEnumerator && SUCCEEDED(pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn)) && uReturn > 0) {
        VARIANT vtProp;
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

    pSvc->Release();
    pLoc->Release();
    pEnumerator->Release();
    CoUninitialize();

    return chassisType;
}

std::string GetHWID() {
    std::string processorId = "";
    try {
        int cpuInfo[4] = { -1 };
        char id[50] = { 0 };
        __cpuid(cpuInfo, 1);
        sprintf_s(id, sizeof(id), "%08X%08X%08X%08X", cpuInfo[0], cpuInfo[1], cpuInfo[2], cpuInfo[3]);
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


std::string devicekind = "Unknown";
std::string token = "null";

using namespace std;
using namespace std::chrono;
const std::string chars = "abcdefghijklmnopqrstuvwxyz";
namespace beast = boost::beast;
namespace websocket = beast::websocket;
namespace asio = boost::asio;
using tcp = boost::asio::ip::tcp;
using json = nlohmann::json;

websocket::stream<tcp::socket>* client = nullptr;
std::unordered_map<std::string, std::string> sclaims;
bool isConnected = false;
bool repeat = false;
bool starting_connection = false;
typedef void (*ServerCallback)(const char* message);
typedef void (*DataStreamBack)(const char* message);
const char* retryUntilSuccess(const char* finall, void (*srvcall)(const char*));


bool TestServer(const std::string& url) {
    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cout << "Failed to initialize curl GGs" << std::endl;
        return false;
    }

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    curl_easy_setopt(curl, CURLOPT_TIMEOUT, 5L);
    curl_easy_setopt(curl, CURLOPT_NOBODY, 1L);

    CURLcode res = curl_easy_perform(curl);
    long http_code = 0;
    if (res == CURLE_OK) {
        curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);
    }

    curl_easy_cleanup(curl);
    return (http_code >= 200 && http_code < 300);
}

const std::string GenRequestAuth(const char* data = "Essence") {
    if (!Check()) return nullptr;
    std::string dataStr = data ? data : "Essence";
    std::string lol = Gen();
    std::string input = H_S1 + ":" + lol + ":" + dataStr + ":" + H_S2;
    unsigned char hash[SHA256_DIGEST_LENGTH];
    EVP_MD_CTX* sha256_ctx = EVP_MD_CTX_new();
    if (sha256_ctx == nullptr) return nullptr;
    if (EVP_DigestInit_ex(sha256_ctx, EVP_sha256(), nullptr) != 1) { EVP_MD_CTX_free(sha256_ctx); return nullptr; }
    if (EVP_DigestUpdate(sha256_ctx, input.c_str(), input.length()) != 1) { EVP_MD_CTX_free(sha256_ctx); return nullptr; }
    if (EVP_DigestFinal_ex(sha256_ctx, hash, nullptr) != 1) { EVP_MD_CTX_free(sha256_ctx); return nullptr; }
    EVP_MD_CTX_free(sha256_ctx);
    std::stringstream request_hash;
    for (int i = 0; i < SHA256_DIGEST_LENGTH; ++i) request_hash << std::hex << (int)hash[i];
    std::string result = lol + "|" + request_hash.str();
    return result;
}

int CheckResponse(const std::string authenticityStr, const std::string originalSeed, const std::string dataStr) {
    if (!Check()) return 666;
    if (authenticityStr.length() < 30) return 1;
    size_t pos = authenticityStr.find("|");
    if (pos == std::string::npos) return 1;
    std::string authenticity2Part = authenticityStr.substr(0, pos); std::string requestHash = authenticityStr.substr(pos + 1);
    size_t colonPos = authenticity2Part.find(":");
    if (colonPos == std::string::npos) return 1;
    std::string chave = authenticity2Part.substr(0, colonPos); std::string n = authenticity2Part.substr(colonPos + 1);
    if (n != originalSeed) return 2;
    std::vector<unsigned char> computedKeyHashBytes = Sha256Hash(K1 + "-" + n);
    std::string computedKeyHash = BytesToHex(computedKeyHashBytes);
    if (computedKeyHash != chave) return 3;
    std::vector<unsigned char> computedHashBytes = Sha256Hash(H_S1 + ":" + chave + ":" + n + ":" + dataStr + ":" + H_S2);
    std::string computedHash = BytesToHex(computedHashBytes);
    if (computedHash != requestHash) return 4;
    return 0;
}

void StartWS(ServerCallback srvcall) {
    isConnected = false;
    if (starting_connection) {
        while (starting_connection)
            std::this_thread::sleep_for(std::chrono::milliseconds(1000));
        return;
    }

    starting_connection = true;

    if (!TestServer("https://essenceapi.discloud.app")) {
        std::cout << "LOOOOOOOOOOL" << std::endl;

        if (!TestServer("http://www.msftconnecttest.com/connecttest.txt"))
            srvcall("pc-offline");
        else
            srvcall("server-offline");

        std::this_thread::sleep_for(std::chrono::milliseconds(1500));
        StartWS(srvcall);
        return;
    }

    asio::io_context ioc;
    tcp::resolver resolver{ ioc };
    websocket::stream<tcp::socket> ws{ ioc };

    try {
        auto const results = resolver.resolve("essenceapi.discloud.app", "ws");
        asio::connect(ws.next_layer(), results.begin(), results.end());

        ws.handshake("essenceapi.discloud.app", "/internals/heartbeat/");
        ws.set_option(websocket::stream_base::decorator(
            [](websocket::request_type& req) {
                req.set("authenticity", "authenticity_token");
                req.set("essence-token", "your_token_here");
            }
        ));

        isConnected = true;
        starting_connection = false;
        srvcall("success");
    }
    catch (std::exception const& e) {
        std::cout << std::string(e.what()) << std::endl;
        srvcall("server-offline");
        std::this_thread::sleep_for(std::chrono::milliseconds(3500));
        StartWS(srvcall);
    }
}



const char* Enc(const char* texto, const char* salt_str = "") {
    if (!Check()) return nullptr;
    std::string salt = salt_str && strlen(salt_str) ? b64url_decode(salt_str) : []() { std::string s(32, '\0'); RAND_bytes((unsigned char*)s.data(), 32); return s; }();
    std::string salt_string = K1 + "-" + b64url_encode(salt), iv = salt.substr(0, 16);
    unsigned char key[EVP_MAX_KEY_LENGTH];
    EVP_Digest(salt_string.c_str(), salt_string.length(), key, nullptr, EVP_sha256(), nullptr);
    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    EVP_EncryptInit_ex(ctx, EVP_aes_256_cbc(), nullptr, key, (unsigned char*)iv.c_str());
    EVP_CIPHER_CTX_set_padding(ctx, 0);
    std::string texto_bytes = add_padding(texto);
    std::vector<unsigned char> encrypted_data(texto_bytes.length());
    int len, encrypted_len;
    EVP_EncryptUpdate(ctx, encrypted_data.data(), &len, (unsigned char*)texto_bytes.c_str(), texto_bytes.length());
    EVP_EncryptFinal_ex(ctx, encrypted_data.data() + len, &(encrypted_len = len));
    EVP_CIPHER_CTX_free(ctx);
    std::string result = b64url_encode(std::string(encrypted_data.begin(), encrypted_data.begin() + len + encrypted_len)) + "." + b64url_encode(salt);
    return _strdup(result.c_str());
}

extern "C" __declspec(dllexport) const char* Dec(const char* encryptedtoken) {
    if (!Check()) return nullptr;
    std::string encrypted_data = b64url_decode(std::string(encryptedtoken).substr(0, std::string(encryptedtoken).find('.')));
    std::string salt = b64url_decode(std::string(encryptedtoken).substr(std::string(encryptedtoken).find('.') + 1));
    std::string iv = salt.substr(0, 16), salt_string = K1 + "-" + b64url_encode(salt);
    unsigned char key[EVP_MAX_KEY_LENGTH];
    EVP_Digest(salt_string.c_str(), salt_string.length(), key, nullptr, EVP_sha256(), nullptr);
    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), nullptr, key, (unsigned char*)iv.c_str());
    EVP_CIPHER_CTX_set_padding(ctx, 0);
    std::vector<unsigned char> decrypted_data(encrypted_data.length() + EVP_MAX_BLOCK_LENGTH);
    int len, decrypted_len;
    EVP_DecryptUpdate(ctx, decrypted_data.data(), &len, (unsigned char*)encrypted_data.c_str(), encrypted_data.length());
    EVP_DecryptFinal_ex(ctx, decrypted_data.data() + len, &(decrypted_len = len));
    EVP_CIPHER_CTX_free(ctx);
    std::string result = remove_padding(std::string(decrypted_data.begin(), decrypted_data.begin() + len + decrypted_len));
    return _strdup(result.c_str());
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
        std::cerr << "Erro ao decodificar JWT: " << ex.what() << std::endl;
        return nullptr;
    }
}

# server_id + port + authenticity + jwt


extern "C" __declspec(dllexport) void DiscordAuth(){
    auth_link = "https://discord.com/api/oauth2/authorize?client_id=1336373573744332963" +
        "&redirect_uri=https://essenceapi.discloud.app/oauth_redirect" + 
        "&response_type=code&scope=email+identify+guilds.join" + 
        "&state=1210371208252760074...{port}...{GenRequestAuth(token)}...{token}";
}


extern "C" __declspec(dllexport) std::chrono::milliseconds TimeLeft(const std::string& file) {
    try {
        // Verifica se o arquivo existe
        DWORD fileAttributes = GetFileAttributesA(file.c_str());  // Usando c_str() aqui
        if (fileAttributes == INVALID_FILE_ATTRIBUTES || (fileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
            return std::chrono::milliseconds(1);
        }

        std::ifstream inFile(file);  // Certifique-se de abrir o arquivo aqui
        if (!inFile.is_open()) {
            return std::chrono::milliseconds(1);
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
            return std::chrono::milliseconds(1);
        }

        std::string result = Dec(dados.c_str());
        std::tm tm = {};
        std::istringstream iss(result);
        iss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

        if (iss.fail()) {
            return std::chrono::milliseconds(1);
        }

        std::time_t savedTime = std::mktime(&tm);
        std::time_t now = std::time(nullptr);
        double difference = std::difftime(savedTime, now);
        if (difference > 0 && difference < 12 * 60 * 60) {
            return std::chrono::milliseconds(static_cast<long long>(difference * 1000));
        }
        return std::chrono::milliseconds(1);
    }
    catch (...) {
        return std::chrono::milliseconds(1);
    }
}

extern "C" __declspec(dllexport) const char* DoJwt(const char* claims) {
    try {
        if (claims != nullptr && std::strlen(claims) > 0) {
            std::string claims_str(claims);
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
        token_builder.set_payload_claim("hwid", jwt::claim(GetHWID()));
        auto token = token_builder.sign(jwt::algorithm::hs256{ K2 });
        char* result = new char[token.size() + 1];
        std::strcpy(result, token.c_str());
        return result;
    }
    catch (...) {
        return nullptr;
    }
}

extern "C" __declspec(dllexport) const char* RequestResource(const char* res, const char* extradata, ServerCallback srvcall, DataStreamBack datastream = nullptr, bool force_return = false) {
    if (!Check()) return nullptr;

    if (!isConnected || !client || !client->is_open()) {
        StartWS(srvcall);
        RequestResource(res, extradata, srvcall, datastream, force_return);
        return "error";
    }

    GetChassisType();

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
            if (datastream != nullptr) {
                datastream(responseMessage.c_str());
                continue;
            }

            json json_response = json::parse(responseMessage);
            if (json_response.contains("authenticity") && json_response.contains("response")) {
                int xd = CheckResponse(original_authenticity, n, responseMessage);
                if (xd == 0) {
                    return "error";
                }

                std::string msg;
                switch (xd) {
                case 1: msg = "missing authenticity values"; break;
                case 2: msg = "response seed does not match request seed"; break;
                case 3: msg = "key pair does not match"; break;
                case 4: msg = "response hash does not match response text"; break;
                }

                return RequestResource(res, extradata, srvcall, datastream, force_return);
            }

            if (force_return) {
                return "error";
            }
            else {
                return RequestResource(res, extradata, srvcall, datastream, force_return);
            }

            buffer.consume(buffer.size());
        }
        catch (std::exception const& e) {
            std::string errorMsg = "[ERROR] Falha ao enviar dados: " + std::string(e.what());

            if (force_return) {
                return "error";
            }

            StartWS(srvcall);
            return RequestResource(res, extradata, srvcall, datastream, force_return);
        }
    }

    return "end";
}

extern "C" __declspec(dllexport) const char* PostReq(const char* final, bool force_return, ServerCallback srvcall) {
    if (!Check()) return nullptr;

    try {
        std::string original_authenticity = GenRequestAuth(token.c_str());

        CURL* curl = curl_easy_init();
        if (!curl) {
            srvcall("library-changed");
            return "erro";
        }

        curl_easy_setopt(curl, CURLOPT_URL, (url + final).c_str());
        curl_easy_setopt(curl, CURLOPT_POST, 1L);
        curl_easy_setopt(curl, CURLOPT_POSTFIELDS, token.c_str());
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, 9L);

        struct curl_slist* headers = nullptr;
        headers = curl_slist_append(headers, "DeviceKind: your_device_kind");
        headers = curl_slist_append(headers, ("authenticity: " + original_authenticity).c_str());
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);

        // Executa a requisição
        CURLcode res = curl_easy_perform(curl);
        long http_code = 0;
        if (res == CURLE_OK) {
            curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);
        }

        if (http_code == 429 && force_return) {
            curl_easy_cleanup(curl);
            return "429";
        }

        if (http_code >= 200 && http_code < 300) {
            char* response_data = nullptr;
            curl_easy_getinfo(curl, CURLINFO_CONTENT_TYPE, &response_data);
            std::string resposta(response_data);

            //if (encrypted) {
                if (resposta.length() < 40) {
                    if (force_return) {
                        curl_easy_cleanup(curl);
                        return "777";
                    }

                    if (repeat) {
                        repeat = false;
                        srvcall("server-off");
                    }
                    else {
                        repeat = true;
                    }

                    return retryUntilSuccess(final, srvcall);
                }

                size_t dotPosResposta = resposta.find('.');
                size_t pipePosOriginal = original_authenticity.find('|');
                size_t dotPosOriginal = original_authenticity.find('.');

                std::string respostaSeed = resposta.substr(dotPosResposta + 1);
                std::string originalSeed = original_authenticity.substr(0, pipePosOriginal);
                originalSeed = originalSeed.substr(dotPosOriginal + 1);

                if (respostaSeed != originalSeed) {
                    if (force_return) {
                        return "erro";
                    }

                    if (repeat) {
                        repeat = false;
                        srvcall("Response seed does not match request seed");
                    }
                    else {
                        repeat = true;
                    }

                    return retryUntilSuccess(final, srvcall);
                }

                std::string re = Dec(resposta.c_str());

                if (re == "Errorr24542") {
                    if (force_return) {
                        return "erro";
                    }

                    srvcall("Client tried to change server response");
                    return retryUntilSuccess(final, srvcall);
                }

                return re.c_str();
            //}
            //else {
            //    return resposta.c_str();
            //}
        }
        else {
            if (force_return) {
                curl_easy_cleanup(curl);
                return "777";
            }

            srvcall("server-off");
            return retryUntilSuccess(final, srvcall);
        }

        curl_easy_cleanup(curl);
    }
    catch (const std::exception& ex) {

        if (!TestServer("http://www.msftconnecttest.com/connecttest.txt")) {
            if (force_return) {
                return "erro";
            }

            srvcall("pc-offline");
            return retryUntilSuccess(final, srvcall);
        }
        else {
            if (force_return) {
                return "erro";
            }

            srvcall("server-offline");
            return retryUntilSuccess(final, srvcall);
        }
    }
}
const char* retryUntilSuccess(const char* finall, ServerCallback srvcall) {
    int hidh = 1;
    while (true) {
        std::this_thread::sleep_for(std::chrono::seconds(2 * hidh));
        const char* lol = PostReq(finall, true, srvcall);
        if (lol != "777" && lol != "erro" && lol != "429") {
            srvcall("success");
            return lol;
        }

        if (hidh < 4) hidh++;
    }
};

extern "C" __declspec(dllexport) void InitServer(const char* t, ServerCallback srvcall) {
    if (!Check()) return;

    token = t;
    devicekind = GetChassisType();
    StartWS(srvcall);
}



BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

