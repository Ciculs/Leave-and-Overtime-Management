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

      // ================= ADMIN / HR =================
      {
        path: "hr-admin",
        name: "DashboardHRAdmin",
        component: DashboardAdmin,
        meta: { roles: ["Admin", "HR"] }
      },

      // ================= HR USER MANAGEMENT =================
      {
        path: "user-management",
        name: "UserManagement",
        component: UserManagement,
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "create-user",
        name: "CreateUser",
        component: CreateUser,
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "assign-manager",
        name: "AssignManager",
        component: AssignManager,
        meta: { roles: ["Admin", "HR"] }
      },

      // ================= MANAGER =================
      {
        path: "manager",
        name: "DashboardManager",
        component: DashboardManager,
        meta: { roles: ["Manager"] }
      },
      {
        path: "team-calendar",
        name: "TeamCalendar",
        component: TeamCalendar,
        meta: { roles: ["Manager"] }
      },
      {
        path: "team-approvals",
        name: "TeamApprovals",
        component: () => import("../views/ManagerApproval.vue"),
        meta: { roles: ["Manager"] }
      },
      {
        path: "ot-manager-approval",
        name: "OTManagerApproval",
        component: () => import("../views/OTManagerApproval.vue"),
        meta: { roles: ["Manager"] }
      },

      // ================= EMPLOYEE =================
      {
        path: "employee",
        name: "DashboardEmployee",
        component: DashboardEmployee,
        meta: { roles: ["Employee"] }
      },
      {
        path: "leave/new",
        name: "LeaveRequest",
        component: LeaveRequest,
        meta: { roles: ["Employee"] }
      },
      {
        path: "my-leaves",
        name: "MyLeaves",
        component: LeaveTable,
        meta: { roles: ["Employee"] }
      },
      {
        path: "my-ot",
        name: "MyOT",
        component: OTList,
        meta: { roles: ["Employee"] }
      },
      {
        path: "ot/edit/:id",
        name: "OTEdit",
        component: OTEdit,
        meta: { roles: ["Employee"] }
      },
      {
        path: "personal-calendar",
        name: "PersonalCalendar",
        component: PersonalCalendar,
        meta: { roles: ["Employee"] }
      },

      // ================= HR / ADMIN =================
      {
        path: "holidays",
        name: "HolidayList",
        component: HolidayList,
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "reports",
        name: "Reports",
        component: Reports,
        meta: { roles: ["Admin", "HR"] }
      },
      {
        path: "hr-approvals",
        name: "HRApprovals",
        component: () => import("../views/HRApproval.vue"),
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

  const requiredRoles = to.matched.find(record => record.meta.roles)?.meta.roles

  if (requiredRoles && !requiredRoles.includes(role)) {
    if (role === "Admin" || role === "HR") return "/hr-admin"
    if (role === "Manager") return "/manager"
    if (role === "Employee") return "/employee"
    return "/login"
  }

  return true
})

export default router