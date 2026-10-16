---
title: "Estándares de Desarrollo .NET y C#"
description: "Convenciones del repositorio para cambios .NET/C# enfocados"
inclusion: "always"
---

# Estándares de Desarrollo .NET y C#

## Arquitectura

- Mantén las entidades de dominio, objetos de valor e invariantes de negocio en `CleanArchitecture.Domain`.
- Coloca los casos de uso, comandos, consultas, handlers, DTOs y abstracciones en `CleanArchitecture.Application`.
- Coloca el acceso a datos y las implementaciones de servicios externos en `CleanArchitecture.Infrastructure`.
- Mantén el enrutamiento HTTP, las responsabilidades de controladores y la composición de la aplicación en `CleanArchitecture.Api`.
- Mantén los contratos de protocolo gRPC y los servicios de transporte en `CleanArchitecture.Proto` y `CleanArchitecture.gRPC`.
- Haz que las dependencias apunten hacia dentro: las capas externas pueden referenciar las internas; el dominio no debe depender de infraestructura ni de proyectos de API.

## Prácticas de C#

- Usa la versión de .NET configurada en el repositorio y conserva habilitados los tipos de referencia anulables.
- Prefiere métodos claros y pequeños, así como abstracciones existentes, antes que código duplicado o frameworks genéricos prematuros.
- Usa APIs `async` para E/S y propaga `CancellationToken` cuando la API circundante lo admita.
- Prefiere inyección por constructor. No instancies dependencias de infraestructura dentro de la lógica de negocio.
- Valida entradas en el límite pertinente y retorna o propaga errores mediante los mecanismos establecidos del proyecto.
- Evita capturas generales de excepciones. Maneja solo las excepciones de las que sea posible recuperarse o traducir significativamente.
- Evita estado estático mutable y no expongas colecciones mutables innecesariamente.

## Datos y Contratos

- Mantén repositorios y adaptadores de infraestructura enfocados en datos o integración externa, no en decisiones de negocio.
- Usa consultas parametrizadas o expresiones ORM; nunca compongas texto de consulta con entrada externa.
- Trata los contratos de API y gRPC como públicos: conserva la compatibilidad hacia atrás salvo que el requisito permita explícitamente un cambio incompatible.
- Actualiza juntos los modelos de solicitud/respuesta, mapeos, validación y pruebas cuando cambie un contrato.

## Pruebas y Validación

- Coloca las pruebas unitarias en el proyecto `CleanArchitecture.*.Tests` correspondiente y sigue las convenciones existentes de xUnit, Shouldly y NSubstitute.
- Prueba el comportamiento observable en lugar de detalles de implementación privados.
- Cubre las rutas de éxito, validación y error afectadas por el cambio.
- Ejecuta primero las pruebas enfocadas; compila la solución completa ante cambios entre proyectos, en el composition root o en contratos públicos.
