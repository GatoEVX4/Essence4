import os
import threading
import time

# Função para renomear os arquivos
def rename_files_in_directory(directory):
    # Obter a lista completa de arquivos para contar o total
    total_files = sum([len(files) for _, _, files in os.walk(directory)])
    
    # Função para exibir o progresso em uma thread separada
    def display_progress():
        nonlocal files_checked
        while files_checked < total_files:
            if total_files > 0:
                progress = (files_checked / total_files) * 100
                print(f"\rProgresso: {progress:.2f}% - Arquivos verificados: {files_checked}/{total_files}", end="")
            time.sleep(0.5)
    
    # Variável compartilhada para contar os arquivos verificados
    files_checked = 0
    
    # Inicia a thread do progresso
    progress_thread = threading.Thread(target=display_progress)
    progress_thread.start()

    # Percorre todos os diretórios e arquivos
    for root, dirs, files in os.walk(directory):
        for file in files:
            if "Essence2" in file:
                old_file_path = os.path.join(root, file)
                new_file_name = file.replace("Essence2", "Essence")
                new_file_path = os.path.join(root, new_file_name)
                os.rename(old_file_path, new_file_path)
            
            # Atualiza o contador de arquivos verificados
            files_checked += 1
    
    # Aguarda a thread do progresso terminar
    progress_thread.join()
    print("\nOperação concluída.")

# Caminho para o diretório a ser processado
diretorio = "C:\\Users\\PC\\Desktop\\Essence"
rename_files_in_directory(diretorio)
