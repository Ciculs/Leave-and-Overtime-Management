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

import UserManagement from "../views/UserManagement.vue"
import CreateUser from "../views/CreateUser.vue"
import AssignManager from "../views/AssignManager.vue"

const routes = [
  {
    path: "/login",
    name: "Login",
    component: Login
  },

  {
    path: "/",
    component: DashboardLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: "",
        redirect: () => {
          const role = localStorage.getItem("role")

          if (role === "Admin" || role === "HR") return "/hr-admin"
          if (role === "Manager") return "/manager"
          if (role === "Employee") return "/employee"

          return "/login"
        }
      },
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

      // ✅ Đã đổi sang mảng roles để cho phép cả Employee và Manager
      {
        path: "leave/new",
        component: LeaveRequest,
        meta: { roles: ["Employee", "Manager"] } 
      },
      { 
        path: "my-leaves", 
        component: LeaveTable, 
        meta: { roles: ["Employee", "Manager"] } 
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
        path: "team-calendar",
        component: TeamCalendar,
        meta: { role: "Manager" }
      },
      {
        path: '/team-approvals',
        name: 'TeamApprovals',
        component: () => import('../views/ManagerApproval.vue')
      },
      {
        path: '/hr-approvals',
        name: 'HRApprovals',
        component: () => import('../views/HRApproval.vue'),
        meta: { role: 'HR' }
      },

      // ✅ ADMIN ROUTES
      {
        path: "hr-admin",
        name: "DashboardAdmin",
        component: () => import("../views/DashboardAdmin.vue"),
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "user-management",
        name: "UserManagement",
        component: () => import("../views/UserManagement.vue"),
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "create-user",
        name: "CreateUser",
        component: () => import("../views/CreateUser.vue"),
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "assign-manager",
        name: "AssignManager",
        component: () => import("../views/AssignManager.vue"),
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "ot-hr-approval",
        name: "OTHRApproval",
        component: () => import("../views/OTHRApproval.vue"),
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "report-dashboard",
        name: "ReportDashboard",
        component: () => import("../views/ReportDashboard.vue"),
        meta: { roles: ["Admin", "HR"] }
      }
    ]
  },

  {
    path: "/:pathMatch(.*)*",
    redirect: "/login"
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

  if (to.matched.some(record => record.meta.requiresAuth) && !token) {
    return "/login"
  }

  if (to.path === "/login" && token) {
    if (role === "Admin" || role === "HR") return "/hr-admin"
    if (role === "Manager") return "/manager"
    if (role === "Employee") return "/employee"
    return "/login"
  }

  // ✅ Đã dọn dẹp logic kiểm tra quyền mượt mà hơn
  if (to.meta.roles) {
    if (!to.meta.roles.includes(role)) return "/login"
  } else if (to.meta.role) {
    if (to.meta.role !== role) return "/login"
  }

  return true
})

export default router