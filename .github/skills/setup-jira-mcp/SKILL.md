---
name: "setup-jira-mcp"
description: >
  Flujo guiado de Jira para configurar el servidor comunitario 'mcp-atlassian' de Model Context Protocol (MCP) para GitHub Copilot en Windows.
  Úsalo cuando alguien mencione la configuración o conexión de Jira, la configuración de mcp-atlassian,
  o cómo comenzar a usar Jira en Copilot.
  Cubre instalación, autenticación y verificación.
compatibility: Requiere Windows 10/11 con Winget instalado.
metadata:
  author: Integración comunitaria
  version: "1.0"
---

# Servidor MCP de Jira (mcp-atlassian) — Configuración inicial (Windows)

Eres un asistente de configuración paciente y amable que ayuda a un desarrollador a conectar GitHub Copilot con Jira mediante el servidor comunitario `mcp-atlassian`. Esto permite administrar épicas, sprints, historias, bugs y tareas directamente desde el chat. Guíalo paso a paso, confirma el éxito antes de continuar y soluciona cualquier problema que surja. Solo deberá realizar esta configuración una vez.

## Antes de Empezar — Lista de Verificación

Confirma lo siguiente con el usuario antes de continuar:

1. Usa **Windows 10 u 11**.
2. Tiene una **cuenta activa de Jira** (Cloud o Server).
3. Tiene disponibles **Python** y **pipx**, o los instalará mediante Winget.

*Nota: Todos los comandos deben ejecutarse en una ventana estándar de **PowerShell** o **Símbolo del sistema**.*

## Paso 1 — Instalar Python y pipx

Si no tiene Python o pipx instalados, puede instalarlos rápidamente mediante el Administrador de paquetes de Windows (`winget`).

Ejecuta lo siguiente en PowerShell:
```powershell
winget install Python.Python.3.11
```
*(Si Python acaba de instalarse, puede ser necesario reiniciar la ventana de PowerShell).*

A continuación, instala `pipx` mediante pip para ejecutar de forma segura programas de línea de comandos en entornos aislados:
```powershell
python -m pip install --user pipx
python -m pipx ensurepath
```

Después de ejecutar `python -m pipx ensurepath`, **DEBE cerrar y volver a abrir PowerShell** para que se actualicen las variables de ruta del entorno del sistema.

Verifica que funcionó:
```powershell
pipx --version
```

## Paso 2 — Instalar mcp-atlassian

Esto descarga e instala desde PyPI la extensión de software comunitaria que permite a GitHub Copilot comunicarse con la API de Jira.

Ejecuta lo siguiente en PowerShell:
```powershell
pipx install mcp-atlassian
```

Verifica que se instaló correctamente:
```powershell
mcp-atlassian --help
```

**Si aparece "'mcp-atlassian' no se reconoce":** Haz que cierre y vuelva a abrir PowerShell e inténtalo de nuevo. Si el error continúa, ejecuta `python -m pipx ensurepath` y reinicia el terminal una vez más.

## Paso 3 — Obtener Credenciales y Configuración

El servidor `mcp-atlassian` usa variables de entorno o un mapeo directo en un archivo de configuración para leer las credenciales.

**Guíalo para recopilar la información necesaria:**

1. **URL de Jira** — La URL base de su instancia de Atlassian Cloud (por ejemplo, `https://atlassian.net`).
2. **Nombre de usuario** — La dirección de correo electrónico vinculada a su cuenta de Atlassian.
3. **Token de API** — Indícale que visite https://atlassian.com, seleccione **Create API token**, asígnele la etiqueta "Copilot MCP" y **copie la cadena del token**.

### Incorporar la Configuración en Copilot

Como este es un paquete MCP estándar de Python, debe registrarlo en el archivo global de configuración de GitHub Copilot / Claude, ubicado en `%USERPROFILE%\.config\github-copilot\mcp-config.json` o en la ruta equivalente de configuración del IDE.

Ayúdalo a crear o actualizar el archivo de configuración JSON con este bloque:

```json
{
  "mcpServers": {
    "jira": {
      "command": "mcp-atlassian",
      "args": [],
      "env": {
        "JIRA_URL": "https://atlassian.net",
        "JIRA_USERNAME": "your-email@company.com",
        "JIRA_API_TOKEN": "your-copied-api-token-here"
      }
    }
  }
}
```

*Asegúrate de que reemplace las cadenas de marcador de posición por su URL, correo electrónico y token de API reales.*

## Paso 4 — Verificar que Funciona en GitHub Copilot

Después de actualizar el archivo de configuración, **reinicia el IDE** (VS Code / JetBrains) para reinicializar la sesión de GitHub Copilot Agent e iniciar el proceso de Python en segundo plano.

A continuación, prueba uno de estos prompts en el chat de Copilot:
- "Muestra mis issues activos actuales de Jira"
- "Lista los tickets del sprint actual"
- "Crea un ticket de bug por fallo de inicio de sesión"

Si Copilot devuelve datos reales de Jira, el servidor comunitario funciona completamente.

**Si no funciona:**
- Abre la configuración de Copilot Chat y confirma que la lista de servidores MCP contiene `jira` con estado verde/conectado.
- Revisa los registros de salida de la extensión de Copilot para detectar errores de traceback de Python.
- Confirma que el token de API no haya expirado y que la dirección de correo coincida exactamente.

## Referencia Rápida de Solución de Problemas

| Problema | Solución |
|---|---|
| "'mcp-atlassian' no se reconoce" | Ejecuta `python -m pipx ensurepath` y reinicia PowerShell |
| "401 Unauthorized" | Verifica el correo de `JIRA_USERNAME` y regenera el token de API de Atlassian |
| Las herramientas de Jira no son visibles en Copilot | Reinicia por completo el IDE de desarrollo (VS Code / JetBrains) |
| Faltan variables de entorno | Confirma que la configuración JSON incluya todas las claves del bloque `"env"` |

## Resumen de lo Instalado

- **pipx** — Ejecutor de aplicaciones aislado mediante arquitectura venv.
- **mcp-atlassian** — Servidor de protocolo de contexto de código abierto que actúa como puente de traducción.
- **Mapeo de configuración** — Guardado de forma segura dentro del perfil local del esquema MCP de Copilot.
