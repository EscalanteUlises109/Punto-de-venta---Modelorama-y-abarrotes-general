# 🔔 Sistema de Alertas de Stock Bajo

## ✅ **IMPLEMENTACIÓN COMPLETADA**

Se ha implementado un sistema automático de alertas que notifica cuando un producto tiene stock crítico (menor al 25% del stock mínimo).

---

## 🎯 **Características del Sistema**

### **1. Detección Inteligente**
- ✅ Calcula automáticamente el **25% del stock mínimo** como umbral crítico
- ✅ Compara el stock actual vs el umbral
- ✅ Alerta solo cuando está por debajo del 25%

### **2. Alerta Visual Atractiva**
- 🎨 **Posición**: Centro superior de la pantalla
- ⏱️ **Duración**: 5 segundos (auto-cierre)
- 🎯 **Información mostrada**:
  - Nombre del producto
  - Stock actual
  - Stock mínimo
  - Barra de progreso visual
- ❌ **Botón de cerrar**: Para cerrar manualmente

### **3. Ubicaciones**
✅ **Página de Productos**: Al cargar la página  
✅ **Punto de Venta**: Al agregar producto al carrito

---

## 📊 **Ejemplo de Cálculo**

### Caso 1: Stock Crítico
```
Producto: Coca Cola 355ml
Stock Mínimo: 10 unidades
Umbral Crítico: 10 × 0.25 = 2.5 unidades
Stock Actual: 2 unidades

2 ≤ 2.5 → ⚠️ ALERTA MOSTRADA
```

### Caso 2: Stock Bajo pero No Crítico
```
Producto: Pan Bimbo
Stock Mínimo: 20 unidades
Umbral Crítico: 20 × 0.25 = 5 unidades
Stock Actual: 6 unidades

6 > 5 → ✅ Sin alerta (está por encima del 25%)
```

### Caso 3: Stock Normal
```
Producto: Leche Lala 1L
Stock Mínimo: 30 unidades
Umbral Crítico: 30 × 0.25 = 7.5 unidades
Stock Actual: 24 unidades

24 > 7.5 → ✅ Sin alerta
```

---

## 🎨 **Apariencia de la Alerta**

```
┌────────────────────────────────────────────┐
│ ⚠️  ⚠️ Stock Bajo!                    [×] │
│                                            │
│ Coca Cola 355ml tiene solo 2 unidades     │
│ (mínimo: 10)                               │
│                                            │
│ ▓▓▓░░░░░░░░░░░░░░░░░░░ 20%                │
└────────────────────────────────────────────┘
```

---

## 🚀 **Cómo Funciona**

### **En Página de Productos**
1. Usuario navega a "Productos"
2. Sistema carga todos los productos
3. **Verificación automática** de stock
4. Si encuentra producto con stock crítico → Muestra alerta
5. Alerta se cierra automáticamente en 5 segundos

### **En Punto de Venta**
1. Usuario agrega producto al carrito (click o escaneo)
2. **Verificación automática** del producto agregado
3. Si tiene stock crítico → Muestra alerta
4. Usuario es consciente del stock bajo antes de vender
5. Alerta se cierra automáticamente en 5 segundos

---

## 📁 **Archivos Creados/Modificados**

### **Nuevo Componente**
1. ✅ `Components/AlertaStockBajo.razor` - Componente reutilizable de alerta

### **Modificados**
2. ✅ `Components/Pages/Productos.razor` - Verificación al cargar
3. ✅ `Components/Pages/Ventas.razor` - Verificación al agregar al carrito

---

## 🎓 **Ventajas del Sistema**

| Ventaja | Beneficio |
|---------|-----------|
| 🎯 **Proactivo** | Alerta antes de que se agote completamente |
| 👁️ **Visual** | Imposible de ignorar, centro de pantalla |
| ⚡ **Automático** | No requiere revisión manual |
| 🎨 **Informativo** | Muestra datos exactos y barra de progreso |
| ⏱️ **No invasivo** | Se cierra solo en 5 segundos |
| 🔄 **Inteligente** | Solo alerta al 25% o menos del mínimo |

---

## 📊 **Escenarios de Uso**

### **Escenario 1: Revisión Matutina**
```
1. Dueño abre la tienda
2. Navega a "Productos"
3. Sistema muestra: "⚠️ Coca Cola tiene solo 2 unidades"
4. Dueño hace nota mental para reponer
```

### **Escenario 2: Venta en Proceso**
```
1. Cliente compra Coca Cola
2. Empleado escanea el código
3. Sistema muestra: "⚠️ Stock crítico: 2 unidades"
4. Empleado informa al gerente
5. Gerente ordena reposición
```

### **Escenario 3: Prevención**
```
Stock Mínimo: 20
Alerta en: 5 unidades (25%)
Acción: Ordenar 15 unidades más
Resultado: Stock repuesto antes de agotarse
```

---

## ⚙️ **Configuración**

### **Cambiar Duración de la Alerta**
En cualquier página que use el componente:
```razor
<AlertaStockBajo DuracionSegundos="10" />  <!-- 10 segundos -->
<AlertaStockBajo DuracionSegundos="3" />   <!-- 3 segundos -->
```

### **Cambiar Umbral Crítico**
Actualmente: **25%** del stock mínimo

Para cambiar, modificar en el código:
```csharp
// Actual: 25%
var umbralCritico = producto.Stock_Minimo * 0.25m;

// Para 30%:
var umbralCritico = producto.Stock_Minimo * 0.30m;

// Para 50%:
var umbralCritico = producto.Stock_Minimo * 0.50m;
```

---

## 🧪 **Pruebas Recomendadas**

### **Test 1: Stock Crítico en Productos**
1. Crear producto con:
   - Stock Mínimo: 20
   - Stock Actual: 4 (20% del mínimo)
2. Ir a página "Productos"
3. ✅ Debe aparecer alerta
4. Esperar 5 segundos
5. ✅ Alerta debe cerrarse automáticamente

### **Test 2: Stock Crítico en Ventas**
1. Producto con stock bajo en sistema
2. Ir a "Ventas"
3. Agregar ese producto al carrito
4. ✅ Debe aparecer alerta
5. Click en [X] para cerrar
6. ✅ Alerta debe cerrarse inmediatamente

### **Test 3: Stock Normal (Sin Alerta)**
1. Producto con:
   - Stock Mínimo: 20
   - Stock Actual: 15
2. Ir a "Productos"
3. ✅ NO debe aparecer alerta
4. Agregar a carrito en Ventas
5. ✅ NO debe aparecer alerta

---

## 💡 **Tips para el Usuario**

1. **Atención**: Cuando veas la alerta, anota el producto para reposición
2. **Urgente**: Si aparece en rojo (< 25%), ordenar inmediatamente
3. **Planificación**: Usar el stock mínimo apropiado para cada producto
4. **Revisión**: Revisar la página de Productos cada mañana

---

## 🔍 **Logs en Consola**

El sistema genera logs para debugging:

```
⚠️ ALERTA: Coca Cola 355ml tiene stock crítico (2/10)
⚠️ ALERTA DE VENTA: Pan Bimbo tiene stock crítico (3/15)
```

---

## 📈 **Mejoras Futuras Posibles**

- [ ] Múltiples alertas (mostrar todos los productos con stock bajo)
- [ ] Sonido de alerta configurable
- [ ] Historial de alertas
- [ ] Envío de notificación por email/SMS
- [ ] Panel de "Productos a Reponer"
- [ ] Sugerencias automáticas de pedido

---

¡Ahora nunca te quedarás sin stock sin saberlo! 🎉
