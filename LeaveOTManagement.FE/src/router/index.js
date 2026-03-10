import { createRouter, createWebHistory } from "vue-router"
import DashboardLayout from "../layouts/DashboardLayout.vue"
import Login from "../views/Login.vue"

import DashboardAdmin from "../views/DashboardAdmin.vue"
import DashboardManager from "../views/DashboardManager.vue"
import DashboardEmployee from "../views/DashboardEmployee.vue"

import LeaveRequest from "../views/LeaveRequest.vue"
import OTList from "../views/OTList.vue"
import OTEdit from "../views/OTEdit.vue"

import HolidayList from "../views/HolidayList.vue"
import Reports from "../views/Reports.vue"
import LeaveTable from "@/views/LeaveTable.vue"
import TeamCalendar from "../views/TeamCalendar.vue"
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
      },
      {
        path: "leave/new",
        component: LeaveRequest,
        meta: { role: "Employee" }
      },
      {
        path: "my-ot",
        component: OTList,
        meta: { role: "Employee" }
      },
      {
        path: "ot/edit/:id",
        component: OTEdit,
        meta: { role: "Employee" }
      },

      // ✅ HR ROUTES
      {
        path: "holidays",
        component: HolidayList,
        meta: { role: "HR" }
      },
      {
        path: "reports",
        component: Reports,
        meta: { role: "HR" }
      },
      { 
        path: "my-leaves", 
        component: LeaveTable, 
        meta: { role: "Employee" } 
      },
      {
        path: "team-calendar",
        component: TeamCalendar,
        meta: { role: "Manager" }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

/* ✅ FIXED ROUTER GUARD */
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token")
  const role = localStorage.getItem("role")

  if (to.matched.some(record => record.meta.requiresAuth) && !token) {
    return next("/login")
  }

  const requiredRole = to.matched.find(r => r.meta.role)?.meta.role

  if (requiredRole && requiredRole !== role) {
    return next("/login")
  }

  next()
})

export default router