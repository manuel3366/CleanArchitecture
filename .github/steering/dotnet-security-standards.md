---
title: "Estándares de Seguridad .NET y C#"
description: "Base de seguridad para el trabajo en funcionalidades .NET/C#"
inclusion: "always"
---

# Estándares de Seguridad .NET y C#

## Entrada y Salida

- Trata la entrada HTTP, gRPC, de configuración, mensajes, archivos y base de datos como no confiable.
- Valida campos obligatorios, formato, rango e identificadores relevantes para autorización en el límite de la aplicación.
- No expongas detalles de excepciones, trazas de pila, secretos, tokens ni datos de infraestructura interna en respuestas de API.
- Codifica o serializa de forma segura los datos no confiables según el contexto de salida. No construyas HTML, SQL, comandos de shell ni rutas de archivo mediante concatenación de cadenas.

## Autorización y Acceso a Datos

- Autentica y autoriza cada operación protegida antes de acceder a datos protegidos.
- Aplica en el servidor las comprobaciones de propiedad, tenant y alcance; nunca confíes en la identidad o los identificadores de tenant proporcionados por el cliente.
- Aplica el principio de mínimo privilegio a servicios, credenciales y consultas de datos.
- Usa exclusivamente enlace de parámetros ORM o comandos parametrizados. Nunca concatentes valores no confiables en SQL.
- Evita la sobreasignación: vincula solo los campos que quien llama puede establecer.

## Secretos y Criptografía

- Nunca agregues secretos, cadenas de conexión, claves de API, tokens de acceso ni claves privadas al control de código fuente, registros, pruebas o respuestas de error.
- Usa los mecanismos de configuración y administración de secretos del repositorio para valores sensibles.
- Usa las APIs criptográficas proporcionadas por el framework y mecanismos de autenticación establecidos; no implementes criptografía propia.

## Seguridad Operacional

- Registra fallos relevantes de seguridad sin guardar datos sensibles.
- Usa cancelación y límites razonables para cargas de trabajo, cargas de archivos y consultas controladas externamente.
- Revisa endpoints y rutas de acceso a datos nuevos en busca de omisiones de autorización, referencias directas inseguras a objetos, inyección y exposición de datos sensibles.
- Agrega pruebas enfocadas para cambios de autorización y validación cuando corresponda.
