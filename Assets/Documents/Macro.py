#!/usr/bin/env python
# coding: utf-8

# In[12]:


import numpy as np
from scipy.optimize import fsolve
import matplotlib.pyplot as plt


# In[13]:


# Variables endógenas 
# Producto y
# precios p
# consumo c
# Inversión I
# Tasa de interés real r
# tipo de cambio seha definido como la cantidad de moneda local por unidad de moneda extranjera


# In[14]:


# Variables exógenas

a = 160  # Consumo autónomo
f = 100    # inversión autónoma
w = 50    # Salario nominal
ka = 30000  # stock de capital 
L = 225    # trabajo
A = 1       # tecnología
x1 = 0.2   # sensibilidad de las exportaciones al ingreso extranjero
x2 = 30  # sensibilidad de las exportaciones al tipo de cambio
m1 = 0.06   # sensibilidad de las importaciones al ingreso 
m2 = 10    # sensibilidad de las importaciones al tipo de cambio
y_ex = 20000  # Ingreso extranjero (PIB extranjero)
r_ex = 0.05   # tasa de interés extranjera


# In[15]:


# Parámetros
c = 0.6     # PMgC
b = 1500   # sensibilidad de la inversión a la tasa de interés
k = 0.2     # sensibilidad de la demanda de dinero a la renta
h = 1000    # sensibilidad  de la demanda de dinero a cambios en la tasa de interés
alpha =0.5 # proporción de la utilización del capital en la producción
rho = 10  # sensibilidad del tipo de cambio a la diferencia entre el tipo de interés local y extranjero
e_0 =  2  # Tipo de cambio autónomo


# In[16]:


# Variables de política. Variables de decisión del jugador (las que él maneja)
# Instrumentos de política fiscal
G = 200     # Gasto Público. Sugerencia hacer variaciones de 100.
t = 0.2     # tasa de impuesto a la renta Esta está entre 0 y 1,  0<t<1

# Instrumentos de política monetaria
# Operaciones de mecado abierto: Compra y venta de bonos.
M = 1600    # Oferta monetaria. Oferta monetaría. sugerencia hacer variaciones de 500 unidades.


# # Pendiente de la curva IS
# La pendiente de la curva IS debe ser negativa por lo tanto como el multiplicador es:
# \begin{equation}
# -\frac{1-c(1-t)+m_1}{(x_2 - m_2)\rho-b}
# \end{equation}
# Debe cumplir que  $ (x_2- m_2)\rho > b$, con $(x_2>m_2)$ y además que $1+m_1 > c(1-t)$

# In[17]:


# definimos algunas simplificaciones
omega = a + f + G + x1*y_ex
gamma = 1-c*(1-t)+m1
q = 1/alpha
u = (1-alpha)/alpha
pi_e = 0      # Iflación esperada
sigma = x2-m2


# In[18]:


def doxn(vars):
    y, r, p = vars
    da = gamma*y + sigma*rho*r - omega - sigma*e_0 - sigma * rho * r_ex
    m  = k*y -h*r - M/p - h*pi_e
    oa = y - ((A**q)*ka*((1-alpha)**u)*((p/w)**u))
    return [da, m, oa]


# In[19]:


# se establece un valor inicial para la solución
y, r, p =  fsolve(doxn, (819, 0.3, 2.5))


# In[20]:


# Using this 'comma' notation, the get the three solutions directly.
print('PIB =', y, 'Precio =', p, 'tasa interés = ', r)


# NOTA: el ganador del juego se determina segun el indice de properidad económica. el cual se calcula apartir de las variabñes de 
#     resultado 
#     INFLación.
#     tasa de desempleo
#     balance prosupuestal
#     
#     los puntos de cada uno dependen de un rango. es decir, por ejemplo si la inflación esta entre 0<\pi<=0.05 tienen 10 puntos, si esta entre
#     0.05<\pi<0.1 tienen 5 pumtos y así.

# In[ ]:


#inflación es la variación de precios.


# # Reportes
# Punto inicial:
# G=200
# PIB = 7287.473075124384 Precio = 24.29157691708128 tasa interés =  1.4163280821392812
# 
# ## Aumento del gasto,
# pasa de 200 a 300
# PIB = 7448.4763527807845 Precio = 24.828254509269282 tasa interés =  1.4494185769357226
# 
# ## Aumento de la tasa impositiva
# nuevamente G = 200 e impiestos pasan de t=0.2 a t=0.3
# PIB = 6792.402057884305 Precio = 22.64134019294768 tasa interés =  1.3143134147702196
# 
# ## Aumento de la oferta monetaria
# nuevamente G=200, t=0.2 y ahora la oferta monetaria pasa de 1000 a 1500
# PIB = 7294.094755485264 Precio = 24.31364918495088 tasa interés =  1.3971252090927304

# In[ ]:


# Definir Saldo Presupuestal o Balance fiscal, SP 
SP = t*y - G


# In[ ]:


# DEfinir el cambio de precios, lo que determinará la inflación.
pi = (p_1 - p_0)/p_0


# In[ ]:


Falta mecanismo para tasa de desempleo
Falta tasa impositiva de inversión (impuesto corporativo)
