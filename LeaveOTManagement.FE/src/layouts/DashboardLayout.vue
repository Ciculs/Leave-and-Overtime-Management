<template>
  <div class="layout">
    <AppSidebar />

    <div class="main">
      <AppHeader />

      <div class="content">
        <router-view />
      </div>

      <AppFooter />
    </div>
  </div>
</template>

<script>
import AppSidebar from "@/components/AppSidebar.vue";
import AppHeader from "@/components/AppHeader.vue";
import AppFooter from "@/components/AppFooter.vue";

export default {
  components: { AppSidebar, AppHeader, AppFooter },
};
</script>

<style scoped>
.layout {
  display: flex;
  height: 100vh;
  background: #f4f6f9;
}

.main {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.content {
  flex: 1;
  padding: 30px;
  overflow-y: auto;
}
</style>