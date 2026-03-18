import { createRouter, createWebHistory } from "vue-router"

import DashboardLayout from "../layouts/DashboardLayout.vue"
import Login from "../views/Login.vue"

import DashboardAdmin from "../views/DashboardAdmin.vue"
import DashboardManager from "../views/DashboardManager.vue"
import DashboardEmployee from "../views/DashboardEmployee.vue"

import LeaveRequest from "../views/LeaveRequest.vue"
import OTList from "../views/OTList.vue"
import OTEdit from "../views/OTEdit.vue"
import PersonalCalendar from "../views/PersonalCalendar.vue"

import HolidayList from "../views/HolidayList.vue"
import Reports from "../views/Reports.vue"

import LeaveTable from "../views/LeaveTable.vue"
import TeamCalendar from "../views/TeamCalendar.vue"

import CreateUser from "../views/CreateUser.vue"
import AssignManager from "../views/AssignManager.vue"

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
      // ================= ADMIN =================
      {
        path: "admin",
        component: DashboardAdmin,
        meta: { role: "Admin" }
      },

      {
        path: "create-user",
        component: CreateUser,
        meta: { role: "HR" }
      },

      {
        path: "assign-manager",
        component: AssignManager,
        meta: { role: "HR" }
      },

      // ================= MANAGER =================
      {
        path: "manager",
        component: DashboardManager,
        meta: { role: "Manager" }
      },

      {
        path: "team-calendar",
        component: TeamCalendar,
        meta: { role: "Manager" }
      },

      {
        path: "team-approvals",
        name: "TeamApprovals",
        component: () => import("../views/ManagerApproval.vue"),
        meta: { role: "Manager" }
      },

      // ================= EMPLOYEE =================
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
        path: "my-leaves",
        component: LeaveTable,
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

      {
        path: "personal-calendar",
        component: PersonalCalendar,
        meta: { role: "Employee" }
      },

      // ================= HR =================
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
        path: "hr-approvals",
        name: "HRApprovals",
        component: () => import("../views/HRApproval.vue"),
        meta: { role: "HR" }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

/* ================= ROUTER GUARD ================= */

router.beforeEach((to) => {
  const token = localStorage.getItem("token")
  const role = localStorage.getItem("role")

  // chưa login nhưng vào trang cần auth
  if (to.matched.some(record => record.meta.requiresAuth) && !token) {
    return "/login"
  }

  // đã login nhưng vào login
  if (to.path === "/login" && token) {
    if (role === "Admin") return "/admin"
    if (role === "Manager") return "/manager"
    if (role === "Employee") return "/employee"
    if (role === "HR") return "/holidays"
  }

  // check role
  const requiredRole = to.matched.find(r => r.meta.role)?.meta.role

  if (requiredRole && requiredRole !== role) {
    return "/login"
  }
})

export default router