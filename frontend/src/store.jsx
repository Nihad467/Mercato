import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { clearSession, request, saveSession } from './api.js';

const StoreContext = createContext(null);
const GUEST_CART_KEY = 'mercato.guestCart';

function readGuestCart() {
  try {
    return JSON.parse(localStorage.getItem(GUEST_CART_KEY) || '[]');
  } catch {
    return [];
  }
}

function normalizeCart(cart) {
  return (cart?.items || []).map((item) => ({
    cartItemId: item.cartItemId,
    productId: item.productId,
    productName: item.productName,
    unitPrice: Number(item.unitPrice || 0),
    quantity: Number(item.quantity || 1),
    totalPrice: Number(item.totalPrice || 0),
    mainImageObjectKey: item.mainImageObjectKey,
  }));
}

function getRoles(accessToken) {
  try {
    const payload = accessToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const claims = JSON.parse(atob(payload.padEnd(Math.ceil(payload.length / 4) * 4, '=')));
    const roles = claims.role || claims.roles || claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || [];
    return (Array.isArray(roles) ? roles : [roles]).map((role) => String(role).toLowerCase());
  } catch {
    return [];
  }
}

export function StoreProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('mercato.accessToken'));
  const [email, setEmail] = useState(() => localStorage.getItem('mercato.email') || '');
  const [cart, setCart] = useState(() => readGuestCart());
  const [wishlist, setWishlist] = useState([]);
  const [toast, setToast] = useState(null);

  const notify = useCallback((message, kind = 'success') => {
    setToast({ message, kind, id: Date.now() });
  }, []);

  useEffect(() => {
    if (!toast) return undefined;
    const timer = window.setTimeout(() => setToast(null), 3400);
    return () => window.clearTimeout(timer);
  }, [toast]);

  useEffect(() => {
    const expireSession = () => {
      setToken(null);
      setEmail('');
      setWishlist([]);
      setCart(readGuestCart());
    };
    window.addEventListener('mercato:session-expired', expireSession);
    return () => window.removeEventListener('mercato:session-expired', expireSession);
  }, []);

  const refreshCart = useCallback(async () => {
    if (!localStorage.getItem('mercato.accessToken')) {
      const guest = readGuestCart();
      setCart(guest);
      return guest;
    }
    const result = await request('/Cart');
    const normalized = normalizeCart(result);
    setCart(normalized);
    return normalized;
  }, []);

  const refreshWishlist = useCallback(async () => {
    if (!localStorage.getItem('mercato.accessToken')) {
      setWishlist([]);
      return [];
    }
    const result = await request('/Wishlist');
    setWishlist(Array.isArray(result) ? result : []);
    return Array.isArray(result) ? result : [];
  }, []);

  useEffect(() => {
    if (token) {
      refreshCart().catch(() => setCart([]));
      refreshWishlist().catch(() => setWishlist([]));
    } else {
      setCart(readGuestCart());
      setWishlist([]);
    }
  }, [token, refreshCart, refreshWishlist]);

  const addToCart = useCallback(async (product, quantity = 1) => {
    if (token) {
      await request('/Cart', { method: 'POST', body: { productId: product.id || product.productId, quantity } });
      await refreshCart();
    } else {
      const items = readGuestCart();
      const productId = product.id || product.productId;
      const existing = items.find((item) => item.productId === productId);
      if (existing) {
        existing.quantity += quantity;
        existing.totalPrice = existing.unitPrice * existing.quantity;
      } else {
        items.push({
          cartItemId: null,
          productId,
          productName: product.name,
          unitPrice: Number(product.price || 0),
          quantity,
          totalPrice: Number(product.price || 0) * quantity,
          categoryName: product.categoryName || '',
          stock: product.stock ?? 99,
        });
      }
      localStorage.setItem(GUEST_CART_KEY, JSON.stringify(items));
      setCart(items);
    }
    notify('Məhsul səbətə əlavə edildi.');
  }, [notify, refreshCart, token]);

  const updateQuantity = useCallback(async (item, quantity) => {
    if (quantity < 1) return;
    if (token && item.cartItemId) {
      await request('/Cart', { method: 'PUT', body: { cartItemId: item.cartItemId, quantity } });
      await refreshCart();
      return;
    }
    const items = readGuestCart().map((entry) => entry.productId === item.productId
      ? { ...entry, quantity, totalPrice: entry.unitPrice * quantity }
      : entry);
    localStorage.setItem(GUEST_CART_KEY, JSON.stringify(items));
    setCart(items);
  }, [refreshCart, token]);

  const removeFromCart = useCallback(async (item) => {
    if (token && item.cartItemId) {
      await request(`/Cart/${item.cartItemId}`, { method: 'DELETE' });
      await refreshCart();
      return;
    }
    const items = readGuestCart().filter((entry) => entry.productId !== item.productId);
    localStorage.setItem(GUEST_CART_KEY, JSON.stringify(items));
    setCart(items);
  }, [refreshCart, token]);

  const signIn = useCallback(async (emailValue, password) => {
    const result = await request('/Auth/login', { method: 'POST', body: { email: emailValue, password } });
    const guestItems = readGuestCart();
    saveSession(result.accessToken, result.refreshToken, emailValue);
    setToken(result.accessToken);
    setEmail(emailValue);
    for (const item of guestItems) {
      await request('/Cart', { method: 'POST', body: { productId: item.productId, quantity: item.quantity } });
    }
    localStorage.removeItem(GUEST_CART_KEY);
    await refreshCart();
    await refreshWishlist();
    return result;
  }, [refreshCart, refreshWishlist]);

  const signOut = useCallback(() => {
    clearSession();
    setToken(null);
    setEmail('');
    setWishlist([]);
    const guest = readGuestCart();
    setCart(guest);
    notify('Hesabdan çıxış edildi.');
  }, [notify]);

  const toggleWishlist = useCallback(async (product) => {
    if (!token) {
      notify('Seçilmişlərə əlavə etmək üçün hesabınıza daxil olun.', 'info');
      return false;
    }
    const id = product.id || product.productId;
    const exists = wishlist.some((item) => item.productId === id);
    if (exists) await request(`/Wishlist/${id}`, { method: 'DELETE' });
    else await request(`/Wishlist/${id}`, { method: 'POST' });
    await refreshWishlist();
    notify(exists ? 'Seçilmişlərdən silindi.' : 'Seçilmişlərə əlavə edildi.');
    return !exists;
  }, [notify, refreshWishlist, token, wishlist]);

  const cartCount = cart.reduce((sum, item) => sum + Number(item.quantity || 0), 0);
  const cartTotal = cart.reduce((sum, item) => sum + Number(item.totalPrice || item.unitPrice * item.quantity || 0), 0);
  const isAdmin = getRoles(token || '').includes('admin');

  const value = useMemo(() => ({
    token, email, cart, wishlist, cartCount, cartTotal, toast, notify, isAdmin,
    addToCart, updateQuantity, removeFromCart, signIn, signOut,
    toggleWishlist, refreshCart, refreshWishlist,
  }), [token, email, cart, wishlist, cartCount, cartTotal, toast, notify, isAdmin, addToCart, updateQuantity, removeFromCart, signIn, signOut, toggleWishlist, refreshCart, refreshWishlist]);

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>;
}

export function useStore() {
  const value = useContext(StoreContext);
  if (!value) throw new Error('useStore must be used within StoreProvider');
  return value;
}
