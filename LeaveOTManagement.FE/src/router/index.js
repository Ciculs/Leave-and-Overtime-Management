import { createRouter, createWebHistory } from "vue-router"
import DashboardLayout from "../layouts/DashboardLayout.vue"

import Login from "../views/Login.vue"
import DashboardAdmin from "../views/DashboardAdmin.vue"
import DashboardManager from "../views/DashboardManager.vue"
import DashboardEmployee from "../views/DashboardEmployee.vue"

const routes = [
  {
    path: "/login",
    component: Login
  },
  {
    path: "/",
    component: DashboardLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: "admin",
        component: DashboardAdmin,
        meta: { role: "Admin" }
      },
      {
        path: "manager",
        component: DashboardManager,
        meta: { role: "Manager" }
      },
      {
        path: "employee",
        component: DashboardEmployee,
        meta: { role: "Employee" }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token")
  const role = localStorage.getItem("role")

  if (to.matched.some(record => record.meta.requiresAuth) && !token) {
    return next("/login")
  }

  if (to.meta.role && to.meta.role !== role) {
    return next("/login")
  }

  next()
})

export default router