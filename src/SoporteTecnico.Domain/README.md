\# SoporteTecnico.Domain



\## Propósito



Esta capa contiene el núcleo del dominio del sistema de soporte técnico. Aquí se definen las entidades principales, los valores que representan los estados y prioridades, y las excepciones relacionadas con las reglas del negocio.



Domain no debe depender de las demás capas de la aplicación.



\## Estructura



```text

SoporteTecnico.Domain/

│

├── Entities/

│   ├── Cliente.cs

│   ├── Tecnico.cs

│   ├── Categoria.cs

│   ├── Ticket.cs

│   └── Seguimiento.cs

│

├── Enums/

│   ├── Prioridad.cs

│   ├── EstadoTicket.cs

│   └── TipoSeguimiento.cs

│

├── Exceptions/

│   └── DomainException.cs

│

└── README.md

```



\## 1. Entities



Contiene las entidades del sistema. Cada clase representa un elemento del dominio y, posteriormente, tendrá sus datos y operaciones correspondientes.



| Archivo | Responsabilidad |

|---|---|

| `Cliente.cs` | Representar a los clientes que solicitan soporte. |

| `Tecnico.cs` | Representar a los técnicos que atienden los tickets. |

| `Categoria.cs` | Representar las categorías de los tickets. |

| `Ticket.cs` | Representar las solicitudes de soporte y controlar su ciclo de vida. |

| `Seguimiento.cs` | Representar los eventos y el historial de cada ticket. |



\## 2. Enums



Contiene enumeraciones que representan conjuntos limitados de valores permitidos.



| Archivo | Responsabilidad |

|---|---|

| `Prioridad.cs` | Definir los niveles Baja, Media, Alta y Critica. |

| `EstadoTicket.cs` | Definir los estados Abierto, Asignado, Resuelto y Cerrado. |

| `TipoSeguimiento.cs` | Definir los tipos de eventos registrados en el historial. |



\## 3. Exceptions



Contiene las excepciones personalizadas relacionadas con incumplimientos de las reglas del dominio.



| Archivo | Responsabilidad |

|---|---|

| `DomainException.cs` | Servir como excepción base para errores específicos del dominio. |



Posteriormente se podrán agregar excepciones derivadas para situaciones concretas, como transiciones de estado inválidas o asignaciones a técnicos no disponibles.



\## Estado actual



La estructura de carpetas y los archivos iniciales están creados.



\- Las enumeraciones ya contienen sus valores.

\- Las entidades todavía no tienen propiedades ni métodos.

\- La excepción base todavía no tiene comportamiento adicional.

\- La lógica de negocio se implementará en una etapa posterior.



\## Regla de organización



El código relacionado con las entidades, sus reglas y los conceptos propios del negocio pertenece a esta capa.



Las operaciones de coordinación de casos de uso, el acceso a datos y la comunicación HTTP corresponden a otras capas de la arquitectura.

