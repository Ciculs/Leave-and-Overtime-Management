<template>
    <div class="profile-card">
        <h3>User Profile</h3>
        <p><strong>Full Name:</strong> {{ fullName }}</p>
        <p><strong>Username:</strong> {{ username }}</p>
        <p><strong>Role:</strong> {{ role }}</p>
    </div>
</template>

<script setup>
const fullName = localStorage.getItem("fullName") || "User"
const username = localStorage.getItem("username") || "N/A"
const role = localStorage.getItem("role") || "Employee"
</script>

<style scoped>
.profile-card {
    background: white;
    padding: 24px;
    border-radius: 16px;
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}
</style>