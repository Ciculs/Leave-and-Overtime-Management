import { createRouter, createWebHistory } from "vue-router"

import DashboardLayout from "../layouts/DashboardLayout.vue"
import Login from "../views/Login.vue"
import ForgotPassword from "../views/ForgotPassword.vue"

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

import UserProfile from "../views/UserProfile.vue"

const routes = [
  {
    path: "/login",
    name: "Login",
    component: Login
  },
  {
    path: "/forgot-password",
    name: "ForgotPassword",
    component: ForgotPassword
  },

  {
    path: "/",
    component: DashboardLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: "",
        redirect: () => {
          const role = (localStorage.getItem("role") || "").trim()

          if (role === "Admin" || role === "HR") return "/hr-admin"
          if (role === "Manager") return "/manager"
          if (role === "Employee") return "/employee"

          return "/login"
        }
      },

      {
        path: "hr-admin",
        name: "DashboardHRAdmin",
        component: DashboardAdmin,
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / Dashboard",
          title: "HR ADMIN Dashboard",
          subtitle: "Overview and management summary"
        }
      },

      {
        path: "user-management",
        name: "UserManagement",
        component: UserManagement,
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / User Management",
          title: "User Management",
          subtitle: "Manage employees, managers and HR accounts"
        }
      },
      {
        path: "create-user",
        name: "CreateUser",
        component: CreateUser,
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / User Management",
          title: "Create User",
          subtitle: "Add a new user account to the system"
        }
      },
      {
        path: "assign-manager",
        name: "AssignManager",
        component: AssignManager,
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / User Management",
          title: "Assign Manager",
          subtitle: "Assign employees to the appropriate manager"
        }
      },

      {
        path: "manager",
        name: "DashboardManager",
        component: DashboardManager,
        meta: {
          roles: ["Manager"],
          section: "Pages / Dashboard",
          title: "Manager Dashboard",
          subtitle: "Overview of team activities and approvals"
        }
      },
      {
        path: "team-calendar",
        name: "TeamCalendar",
        component: TeamCalendar,
        meta: {
          roles: ["Manager"],
          section: "Pages / Team Calendar",
          title: "Team Leave Calendar",
          subtitle: "View approved leave of your team"
        }
      },
      {
        path: "team-approvals",
        name: "TeamApprovals",
        component: () => import("../views/ManagerApproval.vue"),
        meta: {
          roles: ["Manager"],
          section: "Pages / Leave Approvals",
          title: "Leave Approvals",
          subtitle: "Review and approve team leave requests"
        }
      },
      {
        path: "ot-manager-approval",
        name: "OTManagerApproval",
        component: () => import("../views/OTManagerApproval.vue"),
        meta: {
          roles: ["Manager"],
          section: "Pages / OT Approvals",
          title: "OT Approvals",
          subtitle: "Review and approve team overtime requests"
        }
      },

      {
        path: "employee",
        name: "DashboardEmployee",
        component: DashboardEmployee,
        meta: {
          roles: ["Employee"],
          section: "Pages / Dashboard",
          title: "Employee Dashboard",
          subtitle: "Track your leave, overtime and calendar"
        }
      },
      {
        path: "leave/new",
        name: "LeaveRequest",
        component: LeaveRequest,
        meta: {
          roles: ["Employee", "Manager"],
          section: "Pages / My Leave Requests",
          title: "Submit Leave Request",
          subtitle: "Create a new leave request"
        }
      },
      {
        path: "my-leaves",
        name: "MyLeaves",
        component: LeaveTable,
        meta: {
          roles: ["Employee", "Manager"],
          section: "Pages / My Leave Requests",
          title: "My Leave Requests",
          subtitle: "View and track your leave requests"
        }
      },
      {
        path: "my-ot",
        name: "MyOT",
        component: OTList,
        meta: {
          roles: ["Employee", "Manager"],
          section: "Pages / My Overtime",
          title: "Overtime Management",
          subtitle: "Track and manage your extra working hours"
        }
      },
      {
        path: "ot/edit/:id",
        name: "OTEdit",
        component: OTEdit,
        meta: {
          roles: ["Employee", "Manager"],
          section: "Pages / My Overtime",
          title: "Edit OT Request",
          subtitle: "Update your overtime request details"
        }
      },
      {
        path: "personal-calendar",
        name: "PersonalCalendar",
        component: PersonalCalendar,
        meta: {
          roles: ["Employee", "Manager"],
          section: "Pages / Calendar",
          title: "Personal Calendar",
          subtitle: "View your leaves, overtime and holidays"
        }
      },

      {
        path: "holidays",
        name: "HolidayList",
        component: HolidayList,
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / Holiday Calendar",
          title: "Holiday Management",
          subtitle: "Import and manage public holidays"
        }
      },
      {
        path: "reports",
        name: "Reports",
        component: Reports,
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / Reports",
          title: "Report-Dashboard Statistics",
          subtitle: "View reporting and statistics"
        }
      },
      {
        path: "hr-approvals",
        name: "HRApprovals",
        component: () => import("../views/HRApproval.vue"),
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / Leave Approvals",
          title: "HR Leave Approvals",
          subtitle: "Approve or reject leave requests"
        }
      },
      {
        path: "ot-hr-approval",
        name: "OTHRApproval",
        component: () => import("../views/OTHRApproval.vue"),
        meta: {
          roles: ["Admin", "HR"],
          section: "Pages / OT Approvals",
          title: "HR OT Approvals",
          subtitle: "Approve or reject overtime requests"
        }
      },
      {
        path: "profile",
        name: "UserProfile",
        component: UserProfile,
        meta: {
          roles: ["Admin", "HR", "Manager", "Employee"],
          section: "Pages / Profile",
          title: "User Profile",
          subtitle: "View and manage your account information"
        }
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

router.beforeEach((to) => {
  const token = localStorage.getItem("token")
  const role = (localStorage.getItem("role") || "").trim()

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